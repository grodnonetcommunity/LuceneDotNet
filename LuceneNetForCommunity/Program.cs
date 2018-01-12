using System;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.QueryParsers.Surround.Query;
using Lucene.Net.Search;
using Lucene.Net.Spatial.Prefix;
using Lucene.Net.Spatial.Prefix.Tree;
using Lucene.Net.Spatial.Queries;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Spatial4n.Core.Context;
using Spatial4n.Core.Distance;

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

            var context = SpatialContext.GEO;
            int maxLevels = 11;

            SpatialPrefixTree grid = new GeohashPrefixTree(context, maxLevels);
            var strategy = new RecursivePrefixTreeStrategy(grid, "geo");
            
            
            using (Directory directory = new MMapDirectory("index"))
            using (var analyzer = new Lucene.Net.Analysis.Standard.StandardAnalyzer(LuceneVersion.LUCENE_48))
            {
                var config = new IndexWriterConfig(LuceneVersion.LUCENE_48, analyzer);
                using (var ixw = new IndexWriter(directory, config))
                {

                    var shape = context.MakePoint(1, 1);
                    var geoFields = strategy.CreateIndexableFields(shape);
                    var document = new Document
                    {
                        new StringField("id", "1", Field.Store.YES),
                        new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                        new TextField("content", "Hello world", Field.Store.YES),
                        new Int32Field("intValue", 32, Field.Store.YES),
                        new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                        new NumericDocValuesField("docValue", 64),
                        new Field("dateTime", DateTools.DateToString(new DateTime(2018, 1, 1), DateTools.Resolution.SECOND), Field.Store.YES, Field.Index.ANALYZED),
                        new StringField("dateTime", DateTools.DateToString(new DateTime(2017, 1, 1), DateTools.Resolution.SECOND), Field.Store.YES),
                       
                    };
                    foreach (var geoField in geoFields)
                    {
                        document.Add(geoField);
                    }

                    ixw.AddDocument(document);

                    var document2 = new Document
                    {
                        new StringField("id", "2", Field.Store.YES),
                        new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                        new TextField("content", "Hello world 1", Field.Store.YES)
                        {
                            Boost = 1.0f
                        },
                        new Int32Field("intValue", 33, Field.Store.YES),
                        new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                        new NumericDocValuesField("docValue", 65),
                        new StringField("dateTime", DateTools.DateToString(new DateTime(2018, 1, 1), DateTools.Resolution.SECOND), Field.Store.YES)
                    };
                    foreach (var geoField in geoFields)
                    {
                        document2.Add(geoField);
                    }
                    ixw.AddDocument(document2);
                    ixw.Commit();

                    var actualQuery = new BooleanQuery();
                    actualQuery.Add(NumericRangeQuery.NewInt32Range("intValue", 32, 32, true, true), Occur.MUST);
                    using (var ixr = DirectoryReader.Open(directory))
                    {
                        var searcher = new IndexSearcher(ixr);
                        var hits = searcher.Search(actualQuery, 10);
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
