using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
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
using Timer = System.Timers.Timer;

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
                int rows = 50000;

                int tasks = 50;

                if (args.IndexOf("--rows") >= 0)
                {
                    rows = int.Parse(args[args.IndexOf("--rows") + 1]);
                }

                if (args.IndexOf("--tasks") >= 0)
                {
                    tasks = int.Parse(args[args.IndexOf("--tasks") + 1]);
                }

                ProfessionTrain(@"C:\Users\Rojan\Desktop\model.zip", args.Contains("--thread"), tasks, rows);
            }
        }

        private static ReaderWriterLock readerWriterLock = new ReaderWriterLock();

        private static readonly object locker = new object();

        private static volatile bool end;

        private static volatile int wrote;

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

        private static void ProfessionTrain(string modelPath, bool useThread, int tasksCount, int rowsCount)
        {
            Console.Clear();

            const string path = @"C:\Users\Rojan\Desktop\professions.tsv";

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

            //serviceCollection.AddLogging(builder => builder.AddConsole());

            serviceCollection.AddDbContextPool<TaiDbContext>(builder => builder.UseSqlServer("Data Source=192.168.1.100;Initial Catalog=Tai;Persist Security Info=True;User ID=Tai;Password=2cQAT2Yypv8eGgDu", optionsBuilder => optionsBuilder.CommandTimeout(3600)));

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

            Console.WriteLine("Loading from DB and Preparing the Data...");

            ManualResetEvent resetEvent = new ManualResetEvent(false);

            ConcurrentQueue<Article> queue = new ConcurrentQueue<Article>();

            List<int> articles;

            Dictionary<int, string> groups;

            Console.WriteLine("Loading Groups and IDs...");

            using (IServiceScope scope = serviceProvider.CreateScope())
            using (TaiDbContext dbContext = scope.ServiceProvider.GetService<TaiDbContext>())
            {
                groups = dbContext.Groups.AsNoTracking().ToDictionary(group => group.Id, group => group.Identity);

                articles = dbContext.Articles.AsNoTracking().Where(article => article.Language == "Swedish").Select(article => article.Id).ToList();
            }

            void Action()
            {
                using (StreamWriter writer = File.CreateText(path))
                {
                    while (!end)
                    {
                        List<Task> tasks = new List<Task>();

                        while (!queue.IsEmpty)
                        {
                            while (tasks.Count < tasksCount && !queue.IsEmpty)
                            {
                                if (queue.TryDequeue(out Article article))
                                {
                                    tasks.Add(Task.Run(() =>
                                    {
                                        List<string> totalWords = new List<string>();

                                        List<int> totalTags = new List<int>();

                                        List<string> totalLemmas = new List<string>();

                                        foreach (Sentence sentence in article.Sentences)
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
                                                List<Token> tokens = article.Title.TokenizeSentences().SelectMany(list => list).ToList();

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

                                            lock (locker)
                                            {
                                                // ReSharper disable once AccessToDisposedClosure
                                                writer?.WriteLine($"{string.Join(" ", totalWords)}\t{string.Join(" ", totalLemmas)}\t{string.Join(" ", totalTags)}\t{groups[article.GroupId]}");

                                                // ReSharper disable once AccessToDisposedClosure
                                                writer?.Flush();
                                            }
                                        }

                                        wrote++;
                                    }));
                                }
                            }

                            if (tasks.Any())
                            {
                                Task.WaitAll(tasks.ToArray());

                                tasks.ForEach(t => t.Dispose());

                                tasks.Clear();
                            }
                        }
                    }
                }

                Thread.Sleep(1000);

                // ReSharper disable once AccessToDisposedClosure
                resetEvent.Set();
            }

            Console.WriteLine($"Rows: {rowsCount}, Tasks: {tasksCount}...");

            Thread thread = new Thread(o => Action()) { Priority = ThreadPriority.Highest };

            Task task = new Task(Action, TaskCreationOptions.LongRunning);

            if (useThread)
            {
                Console.WriteLine($"Thread with Priority {thread.Priority}...");

                thread.Start();
            }
            else
            {
                Console.WriteLine("Starting the Task...");

                task.Start();
            }

            int loaded = 0;

            int total = articles.Count;

            Console.WriteLine($"Total Articles: {total}, Total Groups: {groups.Count}...");

            int line = 0;

            Stopwatch stopwatch = new Stopwatch();

            Timer timer = new Timer(1000);

            timer.Elapsed += (sender, args) =>
            {
                // ReSharper disable once AccessToModifiedClosure
                Console.CursorTop = line;

                Console.CursorLeft = 0;

                int w = wrote;

                // ReSharper disable once AccessToModifiedClosure
                int l = loaded;

                int t = total;

                int f = w < l ? w : l;

                if (t > 0 && f > 0)
                {
                    Console.WriteLine($"Loaded: {l}/{t}, Wrote: {w}/{t} in {stopwatch.Elapsed:G}, Remaining: {TimeSpan.FromMilliseconds((t - f) * (stopwatch.ElapsedMilliseconds / f)):G}");
                }
                else
                {
                    Console.WriteLine("Waiting for DB...");
                }
            };

            Console.WriteLine("Starting the Process...");

            line = Console.CursorTop;

            timer.Start();

            stopwatch.Start();

            do
            {
                try
                {
                    List<int> segment = articles.Skip(loaded).Take(rowsCount).ToList();

                    List<Article> articleEntities;

                    using (IServiceScope scope = serviceProvider.CreateScope())
                    using (TaiDbContext dbContext = scope.ServiceProvider.GetService<TaiDbContext>())
                    {
                        articleEntities = dbContext.Articles.AsNoTracking().Include(article => article.Sentences).Where(article => segment.Contains(article.Id)).ToList();
                    }

                    articleEntities.ForEach(article => queue.Enqueue(article));

                    loaded += segment.Count;
                }
                catch
                {
                    //
                }

            } while (loaded < total);

            end = true;

            resetEvent.WaitOne();

            resetEvent.Dispose();

            timer.Dispose();

            stopwatch.Stop();

            TextFeaturizingEstimator.Options textOptions = new TextFeaturizingEstimator.Options
            {
                StopWordsRemoverOptions = null,

                KeepNumbers = true
            };

            Console.WriteLine("Shuffling the Training data...");

            IDataView dataView = context.Data.ShuffleRows(context.Data.LoadFromTextFile<ProfData>(path));

            Console.WriteLine("Preparing the Pipeline...");

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

            EstimatorChain<KeyToValueMappingTransformer> estimator2 = estimatorChain

                .Append(context.MulticlassClassification.Trainers.LbfgsMaximumEntropy())

                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            EstimatorChain<KeyToValueMappingTransformer> estimator3 = estimatorChain

                .Append(context.MulticlassClassification.Trainers.SdcaNonCalibrated())

                .Append(context.Transforms.Conversion.MapKeyToValue("PredictedLabel"));

            Console.WriteLine("Training...");

            Console.WriteLine("Model 1 / 3...");

            ITransformer model1 = estimator1.Fit(trainTestSplit.TrainSet);

            Console.WriteLine("Model 2 / 3...");

            ITransformer model2 = estimator2.Fit(trainTestSplit.TrainSet);

            Console.WriteLine("Model 3 / 3...");

            ITransformer model3 = estimator3.Fit(trainTestSplit.TrainSet);

            Console.WriteLine("Evaluating...");

            MulticlassClassificationMetrics testMetrics = context.MulticlassClassification.Evaluate(model1.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data 1");

            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:P2}");

            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:P2}");

            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:P2}");

            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:P2}");

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine();

            context.Model.Save(model1, dataView.Schema, @"C:\Users\Rojan\Desktop\prof-model1.zip");

            testMetrics = context.MulticlassClassification.Evaluate(model2.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data 2");

            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:P2}");

            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:P2}");

            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:P2}");

            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:P2}");

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine();

            context.Model.Save(model2, dataView.Schema, @"C:\Users\Rojan\Desktop\prof-model2.zip");

            testMetrics = context.MulticlassClassification.Evaluate(model3.Transform(trainTestSplit.TestSet));

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine($"*       Metrics for Multi-class Classification model - Test Data 3");

            Console.WriteLine($"*------------------------------------------------------------------------------------------------------------");

            Console.WriteLine($"*       MicroAccuracy:    {testMetrics.MicroAccuracy:P2}");

            Console.WriteLine($"*       MacroAccuracy:    {testMetrics.MacroAccuracy:P2}");

            Console.WriteLine($"*       LogLoss:          {testMetrics.LogLoss:P2}");

            Console.WriteLine($"*       LogLossReduction: {testMetrics.LogLossReduction:P2}");

            Console.WriteLine($"*************************************************************************************************************");

            Console.WriteLine();

            context.Model.Save(model3, dataView.Schema, @"C:\Users\Rojan\Desktop\prof-model3.zip");

            Console.WriteLine("Done!");
        }
    }
}
