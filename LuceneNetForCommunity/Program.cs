using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Lucene.Net.Documents;
using Lucene.Net.Index;
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
            var sw = Stopwatch.StartNew();
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
                    
                    IList<Document> documents = new List<Document>(2000);    
                    for (int i = 0; i < 1000; i++)
                    {
                        var document = new Document
                        {
                            new StringField("id", Guid.NewGuid().ToString(), Field.Store.YES),
                            new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                            new TextField("content", "Hello world", Field.Store.YES),
                            new Int32Field("intValue", 32, Field.Store.YES),
                            new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                            new NumericDocValuesField("docValue", 64)
                        };
                        documents.Add(document);
                        
                    }
                    
                    
                    for (int i = 0; i < 1000; i++)
                    {
                        var document2 = new Document
                        {
                            new StringField("id", Guid.NewGuid().ToString(), Field.Store.YES),
                            new StringField("notTokenized", "Will not be tokenized", Field.Store.YES),
                            new TextField("content", "Hello world 2", Field.Store.YES),
                            new Int32Field("intValue", 33, Field.Store.YES),
                            new Int32Field("intNotStoredValue", 32, Field.Store.NO),
                            new NumericDocValuesField("docValue", 65)

                        };
                        documents.Add(document2);
                        
                    }
                    ixw.AddDocuments(documents);
                    ixw.Commit();
                }
            }
            
            Console.WriteLine(sw.ElapsedMilliseconds);
            Console.ReadKey();
        }
    }
}
