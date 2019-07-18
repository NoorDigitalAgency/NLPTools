using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.ML;
using Microsoft.ML.Calibrators;
using Microsoft.ML.Data;
using Microsoft.ML.Trainers;
using Microsoft.ML.Transforms;
using Microsoft.ML.Transforms.Text;
using NStagger;
using NStaggerExtensions;
using Tai.Data;

namespace AITest
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Contains("--train"))
            {
                Train(@"C:\Users\Rojan\Desktop\model.zip");
            }

            if (args.Contains("--test"))
            {
                Test(@"C:\Users\Rojan\Desktop\model.zip", @"C:\Users\Rojan\Desktop\input.txt");
            }

            if (args.Contains("--prof"))
            {
                Prof(@"C:\Users\Rojan\Desktop\input.txt");
            }
        }

        private static void Test(string modelPath, string inputPath)
        {
            Console.WriteLine("Preparing the AI...");

            MLContext context = new MLContext();

            ITransformer model = context.Model.Load(modelPath, out _);

            PredictionEngine<SentimentData, SentimentPrediction> engine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SUCTagger tagger;

            Console.WriteLine("Loading Tagger...");

            using (FileStream stream = File.Open(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel", FileMode.Open))
            {
                tagger = (SUCTagger)binaryFormatter.Deserialize(stream);
            }

            Console.WriteLine("Done!");

            Console.WriteLine();

            ConsoleColor color = Console.ForegroundColor;

            while (true)
            {
                Console.WriteLine();

                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------");

                Console.WriteLine();

                Console.ForegroundColor = color;

                Console.WriteLine("Press enter when ready!");

                Console.ReadLine();

                string text = File.ReadAllText(inputPath);

                Console.ForegroundColor = ConsoleColor.DarkYellow;

                Console.WriteLine("Processing...");

                Console.WriteLine();

                Console.ForegroundColor = color;

                foreach (string s in text.ToLines())
                {
                    List<Token> tokens = s.TokenizeSentences().SelectMany(list => list).ToList();

                    TaggedToken[] taggedTokens = tokens.Select(token => new TaggedToken(token, null)).ToArray();

                    TaggedToken[] taggedSentence = tagger.TagSentence(taggedTokens, true, false).Where(token => token.Token.Type == TokenType.Latin).ToArray();

                    string[] words = taggedSentence.Select(token => token.LowerCaseText).ToArray();

                    text = string.Join(" ", words);

                    int[] posTags = taggedSentence.Select(token => token.PosTag).ToArray();

                    string tags = string.Join(" ", posTags);

                    string[] lemmaWords = taggedSentence.Select(token => token.Lemma.ToLower()).ToArray();

                    string lemmas = string.Join(" ", lemmaWords);

                    SentimentPrediction predict = engine.Predict(new SentimentData { SentimentText = text, SentimentTags = tags, SentimentLemmas = lemmas });

                    Console.ForegroundColor = predict.Prediction ? ConsoleColor.DarkGreen : ConsoleColor.White;

                    Console.Write(predict.SentimentText);

                    Console.ForegroundColor = ConsoleColor.DarkCyan;

                    Console.WriteLine($" (P: {predict.Probability:P2}, S: {predict.Score})");

                    Console.ForegroundColor = ConsoleColor.White;
                }
            }
        }

        private static void Train(string modelPath)
        {
            Console.WriteLine("Stated!");

            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddDbContext<TaiDbContext>(builder => builder.UseSqlServer("Data Source=192.168.1.100;Initial Catalog=Tai;Persist Security Info=True;User ID=Tai;Password=2cQAT2Yypv8eGgDu"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            List<Article> articles;

            Console.WriteLine("Loading DB Data...");

            using (IServiceScope scope = serviceProvider.CreateScope())
            using (TaiDbContext dbContext = scope.ServiceProvider.GetService<TaiDbContext>())
            {
                articles = dbContext.Articles.Include(article => article.Sentences).ThenInclude(sentence => sentence.Lessons).Where(article => article.Status == ArticleStatus.Completed).ToList();
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SUCTagger tagger;

            Console.WriteLine("Loading Tagger...");

            using (FileStream stream = File.Open(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel", FileMode.Open))
            {
                tagger = (SUCTagger) binaryFormatter.Deserialize(stream);
            }

            List<SentimentData> sentimentData = new List<SentimentData>();

            Console.WriteLine("Preparing Data...");

            foreach (Article article in articles)
            {
                foreach (Sentence sentence in article.Sentences)
                {
                    try
                    {
                        List<Token> tokens = sentence.Text.TokenizeSentences().SelectMany(list => list).ToList();

                        TaggedToken[] taggedTokens = tokens.Select(token => new TaggedToken(token, null)).ToArray();

                        TaggedToken[] taggedSentence = tagger.TagSentence(taggedTokens, true, false).Where(token => token.Token.Type == TokenType.Latin).ToArray();

                        bool label = sentence.Lessons.Count(lesson => lesson.Qualification) > (sentence.Lessons.Count / 2);

                        string[] words = taggedSentence.Select(token => token.LowerCaseText).ToArray();

                        string text = string.Join(" ", words).ToLower();

                        int[] posTags = taggedSentence.Select(token => token.PosTag).ToArray();

                        string tags = string.Join(" ", posTags);

                        string[] lemmaWords = taggedSentence.Select(token => token.Lemma.ToLower()).ToArray();

                        string lemmas = string.Join(" ", lemmaWords).ToLower();

                        sentimentData.Add(new SentimentData {Sentiment = label, SentimentText = text, SentimentTags = tags, SentimentLemmas = lemmas});
                    }
                    catch
                    {
                        //
                    }
                }
            }

            MLContext context = new MLContext();

            Console.WriteLine("Shuffling Data...");

            IDataView dataView = context.Data.ShuffleRows(context.Data.LoadFromEnumerable(sentimentData));

            DataOperationsCatalog.TrainTestData splitDataView = context.Data.TrainTestSplit(dataView, 0.3);

            TextFeaturizingEstimator.Options textOptions = new TextFeaturizingEstimator.Options
            {
                StopWordsRemoverOptions = new CustomStopWordsRemovingEstimator.Options
                {
                    StopWords = new[]
                    {
                        "word",
                        "aderton",
                        "adertonde",
                        "adjö",
                        "aldrig",
                        "all",
                        "alla",
                        "allas",
                        "allt",
                        "alltid",
                        "alltså",
                        "andra",
                        "andras",
                        "annan",
                        "annat",
                        "artonde",
                        "artonn",
                        "att",
                        "av",
                        "bakom",
                        "bara",
                        "behöva",
                        "behövas",
                        "behövde",
                        "behövt",
                        "beslut",
                        "beslutat",
                        "beslutit",
                        "bland",
                        "blev",
                        "bli",
                        "blir",
                        "blivit",
                        "borde",
                        "bort",
                        "borta",
                        "bra",
                        "bäst",
                        "bättre",
                        "båda",
                        "bådas",
                        "både",
                        "dag",
                        "dagar",
                        "dagarna",
                        "dagen",
                        "de",
                        "del",
                        "delen",
                        "dem",
                        "den",
                        "denna",
                        "deras",
                        "dess",
                        "dessa",
                        "det",
                        "detta",
                        "dig",
                        "din",
                        "dina",
                        "dit",
                        "ditt",
                        "dock",
                        "dom",
                        "du",
                        "där",
                        "därför",
                        "då",
                        "efter",
                        "eftersom",
                        "elfte",
                        "eller",
                        "elva",
                        "en",
                        "enkel",
                        "enkelt",
                        "enkla",
                        "enligt",
                        "er",
                        "era",
                        "ert",
                        "ett",
                        "ettusen",
                        "fall",
                        "fanns",
                        "fast",
                        "fem",
                        "femte",
                        "femtio",
                        "femtionde",
                        "femton",
                        "femtonde",
                        "fick",
                        "fin",
                        "finnas",
                        "finns",
                        "fjorton",
                        "fjortonde",
                        "fjärde",
                        "fler",
                        "flera",
                        "flesta",
                        "fram",
                        "framför",
                        "från",
                        "fyra",
                        "fyrtio",
                        "fyrtionde",
                        "få",
                        "får",
                        "fått",
                        "följande",
                        "för",
                        "före",
                        "förlåt",
                        "förra",
                        "första",
                        "ge",
                        "genast",
                        "genom",
                        "ger",
                        "gick",
                        "gjorde",
                        "gjort",
                        "god",
                        "goda",
                        "godare",
                        "godast",
                        "gott",
                        "gälla",
                        "gäller",
                        "gällt",
                        "gärna",
                        "gå",
                        "gång",
                        "går",
                        "gått",
                        "gör",
                        "göra",
                        "ha",
                        "hade",
                        "haft",
                        "han",
                        "hans",
                        "har",
                        "hela",
                        "heller",
                        "hellre",
                        "helst",
                        "helt",
                        "henne",
                        "hennes",
                        "heter",
                        "hit",
                        "hjälp",
                        "hon",
                        "honom",
                        "hundra",
                        "hundraen",
                        "hundraett",
                        "hur",
                        "här",
                        "hög",
                        "höger",
                        "högre",
                        "högst",
                        "i",
                        "ibland",
                        "idag",
                        "igen",
                        "igår",
                        "imorgon",
                        "in",
                        "inför",
                        "inga",
                        "ingen",
                        "ingenting",
                        "inget",
                        "innan",
                        "inne",
                        "inom",
                        "inte",
                        "inuti",
                        "ja",
                        "jag",
                        "jämfört",
                        "kan",
                        "kanske",
                        "knappast",
                        "kolla",
                        "kom",
                        "komma",
                        "kommer",
                        "kommit",
                        "kr",
                        "kunde",
                        "kunna",
                        "kunnat",
                        "kvar",
                        "kör",
                        "legat",
                        "ligga",
                        "ligger",
                        "lika",
                        "likställd",
                        "likställda",
                        "lilla",
                        "lite",
                        "liten",
                        "litet",
                        "lägga",
                        "länge",
                        "längre",
                        "längst",
                        "lätt",
                        "lättare",
                        "lättast",
                        "långsam",
                        "långsammare",
                        "långsammast",
                        "långsamt",
                        "långt",
                        "man",
                        "med",
                        "mellan",
                        "men",
                        "menar",
                        "mer",
                        "mera",
                        "mest",
                        "mig",
                        "min",
                        "mina",
                        "mindre",
                        "minst",
                        "mitt",
                        "mittemot",
                        "mot",
                        "mycket",
                        "många",
                        "måste",
                        "möjlig",
                        "möjligen",
                        "möjligt",
                        "möjligtvis",
                        "ned",
                        "nederst",
                        "nedersta",
                        "nedre",
                        "nej",
                        "ner",
                        "ni",
                        "nio",
                        "nionde",
                        "nittio",
                        "nittionde",
                        "nitton",
                        "nittonde",
                        "nog",
                        "noll",
                        "nr",
                        "nu",
                        "nummer",
                        "när",
                        "nästa",
                        "någon",
                        "någonting",
                        "något",
                        "några",
                        "nån",
                        "nåt",
                        "nödvändig",
                        "nödvändiga",
                        "nödvändigt",
                        "nödvändigtvis",
                        "och",
                        "också",
                        "ofta",
                        "oftast",
                        "olika",
                        "olikt",
                        "om",
                        "oss",
                        "på",
                        "rakt",
                        "redan",
                        "rätt",
                        "sade",
                        "sagt",
                        "samma",
                        "samt",
                        "sedan",
                        "sen",
                        "senare",
                        "senast",
                        "sent",
                        "sex",
                        "sextio",
                        "sextionde",
                        "sexton",
                        "sextonde",
                        "sig",
                        "sin",
                        "sina",
                        "sist",
                        "sista",
                        "siste",
                        "sitt",
                        "sju",
                        "sjunde",
                        "sjuttio",
                        "sjuttionde",
                        "sjutton",
                        "sjuttonde",
                        "själv",
                        "sjätte",
                        "ska",
                        "skall",
                        "skulle",
                        "slutligen",
                        "små",
                        "smått",
                        "snart",
                        "som",
                        "stor",
                        "stora",
                        "stort",
                        "står",
                        "större",
                        "störst",
                        "säga",
                        "säger",
                        "sämre",
                        "sämst",
                        "sätt",
                        "så",
                        "ta",
                        "tack",
                        "tar",
                        "tidig",
                        "tidigare",
                        "tidigast",
                        "tidigt",
                        "till",
                        "tills",
                        "tillsammans",
                        "tio",
                        "tionde",
                        "tjugo",
                        "tjugoen",
                        "tjugoett",
                        "tjugonde",
                        "tjugotre",
                        "tjugotvå",
                        "tjungo",
                        "tolfte",
                        "tolv",
                        "tre",
                        "tredje",
                        "trettio",
                        "trettionde",
                        "tretton",
                        "trettonde",
                        "tro",
                        "tror",
                        "två",
                        "tvåhundra",
                        "under",
                        "upp",
                        "ur",
                        "ursäkt",
                        "ut",
                        "utan",
                        "utanför",
                        "ute",
                        "vad",
                        "var",
                        "vara",
                        "varför",
                        "varifrån",
                        "varit",
                        "varje",
                        "varken",
                        "varsågod",
                        "vart",
                        "vem",
                        "vems",
                        "verkligen",
                        "vet",
                        "vi",
                        "vid",
                        "vidare",
                        "viktig",
                        "viktigare",
                        "viktigast",
                        "viktigt",
                        "vilka",
                        "vilken",
                        "vilket",
                        "vill",
                        "visst",
                        "väl",
                        "vänster",
                        "vänstra",
                        "värre",
                        "vår",
                        "våra",
                        "vårt",
                        "än",
                        "ändå",
                        "ännu",
                        "är",
                        "även",
                        "åtminstone",
                        "åtta",
                        "åttio",
                        "åttionde",
                        "åttonde",
                        "över",
                        "övermorgon",
                        "överst",
                        "övre",
                        "nya",
                        "procent",
                        "ser",
                        "skriver",
                        "tog",
                        "året",
                    }
                }
            };

            TextFeaturizingEstimator.Options tagsOptions = new TextFeaturizingEstimator.Options
            {
                KeepNumbers = true
            };

            EstimatorChain<ColumnConcatenatingTransformer> chain = context.Transforms.Text.FeaturizeText("TextFeatures", textOptions, nameof(SentimentData.SentimentText))

                .Append(context.Transforms.Text.FeaturizeText("TagFeatures", tagsOptions, nameof(SentimentData.SentimentTags)))

                .Append(context.Transforms.Text.FeaturizeText("LemmaFeatures", textOptions, nameof(SentimentData.SentimentLemmas)))

                .Append(context.Transforms.Concatenate("Features", "TextFeatures", "TagFeatures", "LemmaFeatures"));

            EstimatorChain<BinaryPredictionTransformer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>> estimator1 = chain.Append(context.BinaryClassification.Trainers.SdcaLogisticRegression());

            Console.WriteLine("Training...");

            ITransformer model = estimator1.Fit(splitDataView.TrainSet);

            Console.WriteLine("Evaluating...");

            IDataView testDataView = model.Transform(splitDataView.TestSet);

            CalibratedBinaryClassificationMetrics metrics = context.BinaryClassification.Evaluate(testDataView);

            Console.WriteLine();

            Console.WriteLine("Model quality metrics evaluation");

            Console.WriteLine("--------------------------------");

            Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");

            Console.WriteLine($"Auc: {metrics.AreaUnderRocCurve:P2}");

            Console.WriteLine($"F1Score: {metrics.F1Score:P2}");

            Console.WriteLine("=============== End of model evaluation ===============");

            context.Model.Save(model, dataView.Schema, modelPath);
        }

        private static void Prof(string inputPath)
        {
            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddDbContext<TaiDbContext>(builder => builder.UseSqlServer("Data Source=192.168.1.100;Initial Catalog=Tai;Persist Security Info=True;User ID=Tai;Password=2cQAT2Yypv8eGgDu"));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            MLContext context = new MLContext();

            TextFeaturizingEstimator.Options textOptions = new TextFeaturizingEstimator.Options
            {
                StopWordsRemoverOptions = new CustomStopWordsRemovingEstimator.Options
                {
                    StopWords = new[]
                    {
                        "word",
                        "aderton",
                        "adertonde",
                        "adjö",
                        "aldrig",
                        "all",
                        "alla",
                        "allas",
                        "allt",
                        "alltid",
                        "alltså",
                        "andra",
                        "andras",
                        "annan",
                        "annat",
                        "artonde",
                        "artonn",
                        "att",
                        "av",
                        "bakom",
                        "bara",
                        "behöva",
                        "behövas",
                        "behövde",
                        "behövt",
                        "beslut",
                        "beslutat",
                        "beslutit",
                        "bland",
                        "blev",
                        "bli",
                        "blir",
                        "blivit",
                        "borde",
                        "bort",
                        "borta",
                        "bra",
                        "bäst",
                        "bättre",
                        "båda",
                        "bådas",
                        "både",
                        "dag",
                        "dagar",
                        "dagarna",
                        "dagen",
                        "de",
                        "del",
                        "delen",
                        "dem",
                        "den",
                        "denna",
                        "deras",
                        "dess",
                        "dessa",
                        "det",
                        "detta",
                        "dig",
                        "din",
                        "dina",
                        "dit",
                        "ditt",
                        "dock",
                        "dom",
                        "du",
                        "där",
                        "därför",
                        "då",
                        "efter",
                        "eftersom",
                        "elfte",
                        "eller",
                        "elva",
                        "en",
                        "enkel",
                        "enkelt",
                        "enkla",
                        "enligt",
                        "er",
                        "era",
                        "ert",
                        "ett",
                        "ettusen",
                        "fall",
                        "fanns",
                        "fast",
                        "fem",
                        "femte",
                        "femtio",
                        "femtionde",
                        "femton",
                        "femtonde",
                        "fick",
                        "fin",
                        "finnas",
                        "finns",
                        "fjorton",
                        "fjortonde",
                        "fjärde",
                        "fler",
                        "flera",
                        "flesta",
                        "fram",
                        "framför",
                        "från",
                        "fyra",
                        "fyrtio",
                        "fyrtionde",
                        "få",
                        "får",
                        "fått",
                        "följande",
                        "för",
                        "före",
                        "förlåt",
                        "förra",
                        "första",
                        "ge",
                        "genast",
                        "genom",
                        "ger",
                        "gick",
                        "gjorde",
                        "gjort",
                        "god",
                        "goda",
                        "godare",
                        "godast",
                        "gott",
                        "gälla",
                        "gäller",
                        "gällt",
                        "gärna",
                        "gå",
                        "gång",
                        "går",
                        "gått",
                        "gör",
                        "göra",
                        "ha",
                        "hade",
                        "haft",
                        "han",
                        "hans",
                        "har",
                        "hela",
                        "heller",
                        "hellre",
                        "helst",
                        "helt",
                        "henne",
                        "hennes",
                        "heter",
                        "hit",
                        "hjälp",
                        "hon",
                        "honom",
                        "hundra",
                        "hundraen",
                        "hundraett",
                        "hur",
                        "här",
                        "hög",
                        "höger",
                        "högre",
                        "högst",
                        "i",
                        "ibland",
                        "idag",
                        "igen",
                        "igår",
                        "imorgon",
                        "in",
                        "inför",
                        "inga",
                        "ingen",
                        "ingenting",
                        "inget",
                        "innan",
                        "inne",
                        "inom",
                        "inte",
                        "inuti",
                        "ja",
                        "jag",
                        "jämfört",
                        "kan",
                        "kanske",
                        "knappast",
                        "kolla",
                        "kom",
                        "komma",
                        "kommer",
                        "kommit",
                        "kr",
                        "kunde",
                        "kunna",
                        "kunnat",
                        "kvar",
                        "kör",
                        "legat",
                        "ligga",
                        "ligger",
                        "lika",
                        "likställd",
                        "likställda",
                        "lilla",
                        "lite",
                        "liten",
                        "litet",
                        "lägga",
                        "länge",
                        "längre",
                        "längst",
                        "lätt",
                        "lättare",
                        "lättast",
                        "långsam",
                        "långsammare",
                        "långsammast",
                        "långsamt",
                        "långt",
                        "man",
                        "med",
                        "mellan",
                        "men",
                        "menar",
                        "mer",
                        "mera",
                        "mest",
                        "mig",
                        "min",
                        "mina",
                        "mindre",
                        "minst",
                        "mitt",
                        "mittemot",
                        "mot",
                        "mycket",
                        "många",
                        "måste",
                        "möjlig",
                        "möjligen",
                        "möjligt",
                        "möjligtvis",
                        "ned",
                        "nederst",
                        "nedersta",
                        "nedre",
                        "nej",
                        "ner",
                        "ni",
                        "nio",
                        "nionde",
                        "nittio",
                        "nittionde",
                        "nitton",
                        "nittonde",
                        "nog",
                        "noll",
                        "nr",
                        "nu",
                        "nummer",
                        "när",
                        "nästa",
                        "någon",
                        "någonting",
                        "något",
                        "några",
                        "nån",
                        "nåt",
                        "nödvändig",
                        "nödvändiga",
                        "nödvändigt",
                        "nödvändigtvis",
                        "och",
                        "också",
                        "ofta",
                        "oftast",
                        "olika",
                        "olikt",
                        "om",
                        "oss",
                        "på",
                        "rakt",
                        "redan",
                        "rätt",
                        "sade",
                        "sagt",
                        "samma",
                        "samt",
                        "sedan",
                        "sen",
                        "senare",
                        "senast",
                        "sent",
                        "sex",
                        "sextio",
                        "sextionde",
                        "sexton",
                        "sextonde",
                        "sig",
                        "sin",
                        "sina",
                        "sist",
                        "sista",
                        "siste",
                        "sitt",
                        "sju",
                        "sjunde",
                        "sjuttio",
                        "sjuttionde",
                        "sjutton",
                        "sjuttonde",
                        "själv",
                        "sjätte",
                        "ska",
                        "skall",
                        "skulle",
                        "slutligen",
                        "små",
                        "smått",
                        "snart",
                        "som",
                        "stor",
                        "stora",
                        "stort",
                        "står",
                        "större",
                        "störst",
                        "säga",
                        "säger",
                        "sämre",
                        "sämst",
                        "sätt",
                        "så",
                        "ta",
                        "tack",
                        "tar",
                        "tidig",
                        "tidigare",
                        "tidigast",
                        "tidigt",
                        "till",
                        "tills",
                        "tillsammans",
                        "tio",
                        "tionde",
                        "tjugo",
                        "tjugoen",
                        "tjugoett",
                        "tjugonde",
                        "tjugotre",
                        "tjugotvå",
                        "tjungo",
                        "tolfte",
                        "tolv",
                        "tre",
                        "tredje",
                        "trettio",
                        "trettionde",
                        "tretton",
                        "trettonde",
                        "tro",
                        "tror",
                        "två",
                        "tvåhundra",
                        "under",
                        "upp",
                        "ur",
                        "ursäkt",
                        "ut",
                        "utan",
                        "utanför",
                        "ute",
                        "vad",
                        "var",
                        "vara",
                        "varför",
                        "varifrån",
                        "varit",
                        "varje",
                        "varken",
                        "varsågod",
                        "vart",
                        "vem",
                        "vems",
                        "verkligen",
                        "vet",
                        "vi",
                        "vid",
                        "vidare",
                        "viktig",
                        "viktigare",
                        "viktigast",
                        "viktigt",
                        "vilka",
                        "vilken",
                        "vilket",
                        "vill",
                        "visst",
                        "väl",
                        "vänster",
                        "vänstra",
                        "värre",
                        "vår",
                        "våra",
                        "vårt",
                        "än",
                        "ändå",
                        "ännu",
                        "är",
                        "även",
                        "åtminstone",
                        "åtta",
                        "åttio",
                        "åttionde",
                        "åttonde",
                        "över",
                        "övermorgon",
                        "överst",
                        "övre",
                        "nya",
                        "procent",
                        "ser",
                        "skriver",
                        "tog",
                        "året",
                    }
                }
            };

            List<Article> articles;

            Console.WriteLine("Loading DB Data...");

            using (IServiceScope scope = serviceProvider.CreateScope())
            using (TaiDbContext dbContext = scope.ServiceProvider.GetService<TaiDbContext>())
            {
                articles = dbContext.Articles.Include(article => article.Group).Where(article => article.Status == ArticleStatus.Completed).ToList();
            }

            List<ProfData> data = articles.Select(article => new ProfData{Text = article.Text, Group = article.Group.Identity, Title = article.Title}).ToList();

            IDataView dataView = context.Data.LoadFromEnumerable(data);

            DataOperationsCatalog.TrainTestData trainTestSplit = context.Data.TrainTestSplit(dataView, 0.3);

            EstimatorChain<KeyToValueMappingTransformer> estimator = context.Transforms.Conversion.MapValueToKey("Label", nameof(ProfData.Group))

                .Append(context.Transforms.Text.FeaturizeText("TextFeatures", textOptions, nameof(ProfData.Text)))

                .Append(context.Transforms.Text.FeaturizeText("TitleFeatures", textOptions, nameof(ProfData.Title)))

                .Append(context.Transforms.Concatenate("Features", "TextFeatures", "TitleFeatures"))

                .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy())

                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            ITransformer model = estimator.Fit(trainTestSplit.TrainSet);

            MulticlassClassificationMetrics testMetrics = context.MulticlassClassification.Evaluate(model.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");

            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:P2}");

            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:P2}");

            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:P2}");

            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:P2}");

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine();

            PredictionEngine<ProfData, ProfPrediction> engine = context.Model.CreatePredictionEngine<ProfData, ProfPrediction>(model);

            ConsoleColor color = Console.ForegroundColor;

            while (true)
            {
                Console.WriteLine();

                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------------------");

                Console.WriteLine();

                Console.ForegroundColor = color;

                Console.WriteLine("Press enter when ready!");

                Console.ReadLine();

                string text = File.ReadAllText(inputPath);

                ProfPrediction predict = engine.Predict(new ProfData{Text = text, Title = ""});

                dynamic single;

                using (SqlConnection connection = new SqlConnection("Data Source=db.ledigajobb.se;Initial Catalog=Ledigajobb;User ID=ledigajobb;Password=xpAs7N747zjWMGp6;"))
                {
                    single = connection.QuerySingle($"SELECT [Name] FROM [AmfProfessions] WHERE [AmfId] = {predict.Prediction}");
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"{single.Name} ({predict.Prediction})");
            }
        }
    }
}
