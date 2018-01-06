using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace LuceneNetForCommunity
{
    [UsedImplicitly]
    class Program
    {
        [SuppressMessage("ReSharper", "UnusedParameter.Local")]
        static void Main(string[] args)
        {
            // Delete index from previous run
            var directoryName = "index";
            if (System.IO.Directory.Exists(directoryName))
            {
                System.IO.Directory.Delete(directoryName, true);
            }
            
            using (Directory directory = new MMapDirectory("index"))
            using (var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
                using (var ixw = new IndexWriter(directory, config))
                {

                    var document = new Document
                    {
                        new StringField("id", "1", Field.Store.YES),
                        new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                        new TextField("content", "Hello world", Field.Store.YES),
                        new Int32Field("intValue", 32, Field.Store.YES),
                        new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                        new NumericDocValuesField("docValue", 64)
                    };

                    ixw.AddDocument(document);

                    var document2 = new Document
                    {
                        new StringField("id", "2", Field.Store.YES),
                        new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                        new TextField("content", "Hello world 2", Field.Store.YES),
                        new Int32Field("intValue", 33, Field.Store.YES),
                        new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                        new NumericDocValuesField("docValue", 65)
                        
                    };
                    ixw.AddDocument(document2);
                    ixw.Commit();

                    var searchQuery = "1";
                    var query = new MultiFieldQueryParser(LuceneVersion.LUCENE_48, new[] {"id"}, analyzer).Parse(searchQuery);
                    using (var ixr = DirectoryReader.Open(directory))
                    {
                        var searcher = new IndexSearcher(ixr);
                        var hits = searcher.Search(query, 10);
                        PrintHits(hits, searcher);
                    }

                }
            }
        }

        private static void PrintHits(TopDocs docs, IndexSearcher searcher)
        {
            foreach (var scoreDoc in docs.ScoreDocs)
            {
                Console.WriteLine(scoreDoc.Doc);
                var doc = searcher.Doc(scoreDoc.Doc);
                Console.WriteLine($"DocId = {scoreDoc.Doc}, Id = {doc.Get("id")}, Score = {scoreDoc.Score}");
            }
        }
    }
}
