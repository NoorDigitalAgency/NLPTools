using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
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
                Test(@"C:\Users\Rojan\Desktop\model.zip");
            }

            if (args.Contains("--prof"))
            {
                Prof(@"C:\Users\Rojan\Desktop\model.zip");
            }
        }

        private static void Test(string modelPath)
        {
            string[] stopWords = {
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
            };

            Console.WriteLine("Loading the Ads...");

            List<string> ads;

            using (SqlConnection connection = new SqlConnection("Data Source=db.ledigajobb.se;Initial Catalog=Ledigajobb;User ID=ledigajobb;Password=xpAs7N747zjWMGp6;"))
            {
                ads = connection.Query(@"SELECT [t1].[Text] FROM [JobAds] AS [t0] INNER JOIN [JobAdsDetails] AS [t1] ON [t1].[Id] = [t0].[DetailsId] WHERE CONVERT(DATE, [t1].[PublishedDate]) = @Today", new { Today = DateTime.Now.Date }).Select(o => o.Text as string).ToList();
            }

            Console.WriteLine("Preparing the AI...");

            MLContext context = new MLContext();

            List<PredictionEngine<SentimentData, SentimentPrediction>> engines = new List<PredictionEngine<SentimentData, SentimentPrediction>>();

            using (ZipArchive zipArchive = new ZipArchive(File.OpenRead(modelPath), ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    ITransformer model;

                    using (Stream stream = entry.Open())
                    {
                        model = context.Model.Load(stream, out _);
                    }

                    PredictionEngine<SentimentData, SentimentPrediction> engine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);

                    engines.Add(engine);
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SUCTagger tagger;

            Console.WriteLine("Loading Tagger...");

            using (FileStream stream = File.Open(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel", FileMode.Open))
            {
                tagger = (SUCTagger)binaryFormatter.Deserialize(stream);
            }

            Console.WriteLine("Processing the Ads...");

            using (StreamWriter writer = File.CreateText(@"C:\Users\Rojan\Desktop\ads.html"))
            {
                writer.Write("<!doctype html><html lang=\"sv\"><head><meta charset=\"utf-8\"><title>Ads</title></head><body>\r\n");

                foreach (string ad in ads)
                {
                    List<string> keywords = new List<string>();

                    writer.WriteLine("<p style=\"background-color: #333333; margin: 20px 0; padding: 10px;\">");

                    foreach (string s in ad.ToLines())
                    {
                        List<Token> tokens = s.TokenizeSentences().SelectMany(list => list).ToList();

                        TaggedToken[] taggedTokens = tokens.Select(token => new TaggedToken(token, null)).ToArray();

                        TaggedToken[] taggedSentence = tagger.TagSentence(taggedTokens, true, false).Where(token => token.Token.Type == TokenType.Latin).ToArray();

                        string[] words = taggedSentence.Select(token => token.LowerCaseText).ToArray();

                        var text = string.Join(" ", words);

                        int[] posTags = taggedSentence.Select(token => token.PosTag).ToArray();

                        string tags = string.Join(" ", posTags);

                        string[] lemmaWords = taggedSentence.Select(token => token.Lemma.ToLower()).ToArray();

                        string lemmas = string.Join(" ", lemmaWords);

                        SentimentData data = new SentimentData { SentimentText = text, SentimentTags = tags, SentimentLemmas = lemmas };

                        List<SentimentPrediction> predictions = engines.Select(engine => engine.Predict(data)).ToList();

                        List<SentimentPrediction> positives = predictions.Where(sentimentPrediction => sentimentPrediction.Prediction).ToList();

                        bool prediction = positives.Count > (engines.Count / 2);

                        string line = $"<span style=\"color: {(prediction ? "#84bf40" : "#bfbfbf")}\">{data.SentimentText}</span>&nbsp;" +

                                      $"<span style=\"color: #00bcd4\">(P: {(prediction ? positives.Average(sentimentPrediction => sentimentPrediction.Probability) : predictions.Average(sentimentPrediction => sentimentPrediction.Probability)):P2}, " +

                                      $"S: {(prediction ? positives.Average(sentimentPrediction => sentimentPrediction.Score) : predictions.Average(sentimentPrediction => sentimentPrediction.Score))})</span><br />";

                        writer.WriteLine(line);

                        if (prediction)
                        {
                            for (int i = 0; i < words.Length; i++)
                            {
                                if (!stopWords.Contains(words[i]))
                                {
                                    keywords.Add($"({words[i]}&nbsp;:&nbsp;{lemmaWords[i]}&nbsp;:&nbsp;{posTags[i]})<br />");
                                }
                            }
                        }
                    }

                    if (keywords.Any())
                    {
                        writer.WriteLine("<span style=\"padding-top: 20px; color: #ff9800; display: inline-block;\">");

                        writer.WriteLine(string.Join("", keywords));

                        writer.WriteLine("</span>");
                    }

                    writer.WriteLine("</p>");

                    writer.Flush();
                }

                writer.Write("</body></html>\r\n");
            }

            Console.WriteLine("Done!");
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
                articles = dbContext.Articles.Include(article => article.ArticleUsers).Include(article => article.Sentences).ThenInclude(sentence => sentence.Lessons)

                    .Where(article => article.Status == ArticleStatus.Completed && article.ArticleUsers.All(user => user.Archived != true) && article.ArticleUsers.Count % 2 > 0).ToList();
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SUCTagger tagger;

            Console.WriteLine("Loading Tagger...");

            using (FileStream stream = File.Open(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel", FileMode.Open))
            {
                tagger = (SUCTagger)binaryFormatter.Deserialize(stream);
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

                        sentimentData.Add(new SentimentData { Sentiment = label, SentimentText = text, SentimentTags = tags, SentimentLemmas = lemmas });
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
                StopWordsRemoverOptions = null /*new CustomStopWordsRemovingEstimator.Options
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
                }*/
            };

            TextFeaturizingEstimator.Options tagsOptions = new TextFeaturizingEstimator.Options
            {
                KeepNumbers = true
            };

            EstimatorChain<ColumnConcatenatingTransformer> chain = context.Transforms.Text.FeaturizeText("TextFeatures", textOptions, nameof(SentimentData.SentimentText))

                .Append(context.Transforms.Text.FeaturizeText("TagFeatures", tagsOptions, nameof(SentimentData.SentimentTags)))

                .Append(context.Transforms.Text.FeaturizeText("LemmaFeatures", textOptions, nameof(SentimentData.SentimentLemmas)))

                .Append(context.Transforms.Concatenate("Features", "TextFeatures", "TagFeatures", "LemmaFeatures"))

                .AppendCacheCheckpoint(context);

            EstimatorChain<BinaryPredictionTransformer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>> estimator1 = chain.Append(context.BinaryClassification.Trainers.SdcaLogisticRegression());

            EstimatorChain<BinaryPredictionTransformer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>> estimator2 = chain.Append(context.BinaryClassification.Trainers.LbfgsLogisticRegression());

            EstimatorChain<BinaryPredictionTransformer<CalibratedModelParametersBase<LinearBinaryModelParameters, PlattCalibrator>>> estimator3 = chain.Append(context.BinaryClassification.Trainers.SgdCalibrated());

            EstimatorChain<FieldAwareFactorizationMachinePredictionTransformer> estimator4 = chain.Append(context.BinaryClassification.Trainers.FieldAwareFactorizationMachine());

            EstimatorChain<BinaryPredictionTransformer<PriorModelParameters>> estimator5 = chain.Append(context.BinaryClassification.Trainers.Prior());

            Console.WriteLine("Training...");

            ITransformer model1 = estimator1.Fit(splitDataView.TrainSet);

            ITransformer model2 = estimator2.Fit(splitDataView.TrainSet);

            ITransformer model3 = estimator3.Fit(splitDataView.TrainSet);

            ITransformer model4 = estimator4.Fit(splitDataView.TrainSet);

            ITransformer model5 = estimator5.Fit(splitDataView.TrainSet);

            Console.WriteLine("Evaluating...");

            Dictionary<string, ITransformer> entries = new Dictionary<string, ITransformer> { { "Model 1", model1 }, { "Model 2", model2 }, { "Model 3", model3 }, { "Model 4", model4 }, { "Model 5", model5 } };

            using (ZipArchive zipArchive = new ZipArchive(File.Create(modelPath), ZipArchiveMode.Create))
            {
                foreach ((var key, ITransformer value) in entries)
                {
                    Console.WriteLine();

                    Console.WriteLine($"Model quality metrics evaluation for {key}");

                    Console.WriteLine("--------------------------------");

                    try
                    {
                        IDataView testDataView = value.Transform(splitDataView.TestSet);

                        CalibratedBinaryClassificationMetrics metrics = context.BinaryClassification.Evaluate(testDataView);

                        Console.WriteLine($"Accuracy: {metrics.Accuracy:P2}");

                        Console.WriteLine($"Auc: {metrics.AreaUnderRocCurve:P2}");

                        Console.WriteLine($"F1Score: {metrics.F1Score:P2}");

                        ZipArchiveEntry zipArchiveEntry = zipArchive.CreateEntry(key);

                        using (Stream stream = zipArchiveEntry.Open())
                        {
                            context.Model.Save(model1, dataView.Schema, stream);

                            stream.Flush();
                        }
                    }
                    catch
                    {
                        Console.WriteLine("¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤¤");
                    }

                    Console.WriteLine("=============== End of model evaluation ===============");
                }
            }
        }

        private static void Prof(string modelPath)
        {
            string[] stopWords = {
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
            };

            ServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddDbContext<TaiDbContext>(builder => builder.UseSqlServer("Data Source=192.168.1.100;Initial Catalog=Tai;Persist Security Info=True;User ID=Tai;Password=2cQAT2Yypv8eGgDu", optionsBuilder => optionsBuilder.CommandTimeout(3600)));

            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            Console.WriteLine("Loading AI Models...");

            MLContext context = new MLContext();

            List<PredictionEngine<SentimentData, SentimentPrediction>> engines = new List<PredictionEngine<SentimentData, SentimentPrediction>>();

            using (ZipArchive zipArchive = new ZipArchive(File.OpenRead(modelPath), ZipArchiveMode.Read))
            {
                foreach (ZipArchiveEntry entry in zipArchive.Entries)
                {
                    ITransformer transformer;

                    using (Stream stream = entry.Open())
                    {
                        transformer = context.Model.Load(stream, out _);
                    }

                    PredictionEngine<SentimentData, SentimentPrediction> predictionEngine = context.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(transformer);

                    engines.Add(predictionEngine);
                }
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();

            SUCTagger tagger;

            Console.WriteLine("Loading Tagger...");

            using (FileStream stream = File.Open(@"C:\Users\Rojan\Downloads\swedish.nmodel\swedish.nmodel", FileMode.Open))
            {
                tagger = (SUCTagger)binaryFormatter.Deserialize(stream);
            }

            TextFeaturizingEstimator.Options textOptions = new TextFeaturizingEstimator.Options
            {
                StopWordsRemoverOptions = null,

                KeepNumbers = true
            };

            List<int> articles;

            Dictionary<int, string> groups;

            Dictionary<int, string> titles;

            Dictionary<int, Sentence[]> sentences;

            Console.WriteLine("Loading DB Data...");

            using (IServiceScope scope = serviceProvider.CreateScope())
            using (TaiDbContext dbContext = scope.ServiceProvider.GetService<TaiDbContext>())
            {
                groups = dbContext.Groups.ToDictionary(group => group.Id, group => group.Identity);

                articles = dbContext.Articles.Select(article => article.Id).ToList();

                titles = dbContext.Articles.ToDictionary(article => article.Id, article => article.Title);

                sentences = dbContext.Sentences.ToList().GroupBy(sentence => sentence.ArticleId).ToDictionary(grouping => grouping.Key, grouping => grouping.ToArray());
            }

            Console.WriteLine("Preparing the Ads...");

            List<ProfData> data = new List<ProfData>();

            foreach (int article in articles)
            {
                List<string> totalWords = new List<string>();

                List<int> totalTags = new List<int>();

                List<string> totalLemmas = new List<string>();

                foreach (Sentence sentence in sentences[article])
                {
                    try
                    {
                        List<Token> tokens = sentence.Text.TokenizeSentences().SelectMany(list => list).ToList();

                        TaggedToken[] taggedTokens = tokens.Select(token => new TaggedToken(token, null)).ToArray();

                        TaggedToken[] taggedSentence = tagger.TagSentence(taggedTokens, true, false).Where(token => token.Token.Type == TokenType.Latin).ToArray();

                        string[] words = taggedSentence.Select(token => token.LowerCaseText).ToArray();

                        string text = string.Join(" ", words).ToLower();

                        int[] posTags = taggedSentence.Select(token => token.PosTag).ToArray();

                        string tags = string.Join(" ", posTags);

                        string[] lemmaWords = taggedSentence.Select(token => token.Lemma.ToLower()).ToArray();

                        string lemmas = string.Join(" ", lemmaWords).ToLower();

                        SentimentData sentimentData = new SentimentData { SentimentText = text, SentimentTags = tags, SentimentLemmas = lemmas };

                        List<SentimentPrediction> predictions = engines.Select(predictionEngine => predictionEngine.Predict(sentimentData)).ToList();

                        List<SentimentPrediction> positives = predictions.Where(sentimentPrediction => sentimentPrediction.Prediction).ToList();

                        bool prediction = positives.Count > (engines.Count / 2);

                        if (prediction)
                        {
                            for (int i = 0; i < words.Length; i++)
                            {
                                if (!stopWords.Contains(words[i]))
                                {
                                    totalWords.Add(words[i]);

                                    totalTags.Add(posTags[i]);

                                    totalLemmas.Add(lemmaWords[i]);
                                }
                            }
                        }
                    }
                    catch
                    {
                        //
                    }
                }

                if (totalWords.Any())
                {
                    try
                    {
                        List<Token> tokens = titles[article].TokenizeSentences().SelectMany(list => list).ToList();

                        TaggedToken[] taggedTokens = tokens.Select(token => new TaggedToken(token, null)).ToArray();

                        TaggedToken[] taggedSentence = tagger.TagSentence(taggedTokens, true, false).Where(token => token.Token.Type == TokenType.Latin).ToArray();

                        string[] words = taggedSentence.Select(token => token.LowerCaseText).ToArray();

                        int[] posTags = taggedSentence.Select(token => token.PosTag).ToArray();

                        string[] lemmaWords = taggedSentence.Select(token => token.Lemma.ToLower()).ToArray();

                        for (int i = 0; i < words.Length; i++)
                        {
                            if (!stopWords.Contains(words[i]))
                            {
                                totalWords.Add(words[i]);

                                totalTags.Add(posTags[i]);

                                totalLemmas.Add(lemmaWords[i]);
                            }
                        }
                    }
                    catch
                    {
                        //
                    }

                    data.Add(new ProfData { Group = groups[article], Text = string.Join(" ", totalWords), Tags = string.Join(" ", totalTags), Lemma = string.Join(" ", totalLemmas) });
                }
            }

            Task.Run(() =>
            {
                try
                {
                    BinaryFormatter formatter = new BinaryFormatter();

                    formatter.Serialize(File.Create(@"C:\Users\Rojan\Desktop\prof.data"), data);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            Console.WriteLine("Preparing the Training data...");

            IDataView dataView = context.Data.ShuffleRows(context.Data.LoadFromEnumerable(data));

            DataOperationsCatalog.TrainTestData trainTestSplit = context.Data.TrainTestSplit(dataView, 0.3);

            EstimatorChain<ColumnConcatenatingTransformer> estimatorChain = context.Transforms.Conversion.MapValueToKey("Label", nameof(ProfData.Group))

                .Append(context.Transforms.Text.FeaturizeText("TextFeatures", textOptions, nameof(ProfData.Text)))

                .Append(context.Transforms.Text.FeaturizeText("TagsFeatures", textOptions, nameof(ProfData.Tags)))

                .Append(context.Transforms.Text.FeaturizeText("LemmaFeatures", textOptions, nameof(ProfData.Lemma)))

                .Append(context.Transforms.Concatenate("Features", "TextFeatures", "TagsFeatures", "LemmaFeatures"))

                .AppendCacheCheckpoint(context);

            EstimatorChain<KeyToValueMappingTransformer> estimator1 = estimatorChain

                .Append(context.MulticlassClassification.Trainers.SdcaMaximumEntropy())

                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));


            Console.WriteLine("Training...");

            ITransformer model1 = estimator1.Fit(trainTestSplit.TrainSet);

            Console.WriteLine("Evaluating...");

            MulticlassClassificationMetrics testMetrics = context.MulticlassClassification.Evaluate(model1.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data     ");

            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:P2}");

            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:P2}");

            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:P2}");

            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:P2}");

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine();

            context.Model.Save(model1, dataView.Schema, @"C:\Users\Rojan\Desktop\prof-model.zip");

            /*PredictionEngine<ProfData, ProfPrediction> engine = context.Model.CreatePredictionEngine<ProfData, ProfPrediction>(model1);

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

                ProfPrediction predict = engine.Predict(new ProfData { Text = text, Lemma = "" });

                dynamic single;

                using (SqlConnection connection = new SqlConnection("Data Source=db.ledigajobb.se;Initial Catalog=Ledigajobb;User ID=ledigajobb;Password=xpAs7N747zjWMGp6;"))
                {
                    single = connection.QuerySingle($"SELECT [Name] FROM [AmfProfessions] WHERE [AmfId] = {predict.Prediction}");
                }

                Console.ForegroundColor = ConsoleColor.Green;

                Console.WriteLine($"{single.Name} ({predict.Prediction})");
            }*/
        }
    }
}
