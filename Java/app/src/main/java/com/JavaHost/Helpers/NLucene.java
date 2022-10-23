package com.JavaHost.Helpers;

import org.apache.lucene.analysis.Analyzer;
import org.apache.lucene.analysis.cn.smart.SmartChineseAnalyzer;
import org.apache.lucene.document.*;
import org.apache.lucene.index.*;


import org.apache.lucene.queryparser.classic.MultiFieldQueryParser;
import org.apache.lucene.queryparser.classic.QueryParser;
import org.apache.lucene.search.IndexSearcher;
import org.apache.lucene.search.Query;
import org.apache.lucene.search.ScoreDoc;
import org.apache.lucene.search.TopDocs;
import org.apache.lucene.store.*;



import java.nio.file.Path;
import java.util.List;
import java.util.function.Predicate;


// Lucene介绍与使用 https://blog.csdn.net/weixin_42633131/article/details/82873731/
public class NLucene {

    IndexWriter indexWriter ;
    IndexWriterConfig conf;

    Directory directory;
    Analyzer analyzer;
    IndexSearcher isearcher ;
    DirectoryReader ireader ;
    QueryParser parser ;

    public void Load (String baseDir)
    {

        try{
             Dispose();
            //2 索引目录类,指定索引在硬盘中的位置
            directory = FSDirectory.open(Path.of(baseDir));


        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }


    public  void PrepareStore()
    {
        try{
            //3 创建分词器对象
            analyzer = new SmartChineseAnalyzer();
            //4 索引写出工具的配置对象
            conf = new IndexWriterConfig(analyzer);
            conf.setOpenMode( IndexWriterConfig.OpenMode.CREATE_OR_APPEND) ;
            //5 创建索引的写出工具类。参数：索引的目录和配置信息
            indexWriter = new IndexWriter(directory, conf);
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }

    public void PrepareSearch()
    {
        try{
            ireader = DirectoryReader.open(directory);
            isearcher = new IndexSearcher(ireader);
        // Parse a simple query that searches for "text":
        //  parser = new QueryParser("fieldname", analyzer);

        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }


    public void Search(String searchText, int topN, Predicate<Document> GetPerDoc,String... fieldNames)
    {
        try {
            QueryParser parser = null ;
            if(fieldNames.length==1)
            {
                // 创建查询解析器,两个参数：默认要查询的字段的名称，分词器
                  parser = new QueryParser(fieldNames[0], this.analyzer);
            }
            else if(fieldNames.length>1)
            {
                parser = new MultiFieldQueryParser(fieldNames,analyzer) ;
            }
            else
            {
                throw  new Exception("fieldnames 不能为空");
            }
            // 创建查询对象
            Query query = parser.parse(searchText);
            // 搜索数据,两个参数：查询条件对象要查询的最大结果条数
            // 返回的结果是 按照匹配度排名得分前N名的文档信息（包含查询到的总条数信息、所有符合条件的文档的编号信息）。
            TopDocs topDocs =  isearcher.search(query, topN);
            // 获取总条数
            System.out.println("本次搜索共找到" + topDocs.totalHits + "条数据");
            // 获取得分文档对象（ScoreDoc）数组.SocreDoc中包含：文档的编号、文档的得分
            ScoreDoc[] scoreDocs = topDocs.scoreDocs;
            for (ScoreDoc scoreDoc : scoreDocs) {
                // 取出文档编号
                int docID = scoreDoc.doc;
                // 根据编号去找文档
                Document doc = ireader.document(docID);
                GetPerDoc.test(doc);
                // 取出文档得分
                System.out.println("得分： " + scoreDoc.score);
            }

        } catch (Exception e) {
             e.printStackTrace();
        }
    }
    

    public IndexWriter GetIndexWriter()
    {
        return this.indexWriter ;
    }

    public  IndexSearcher GetIndexSearcher()
    {
        return this.isearcher ;
    }


    public void Save(Document... doc)
    {
        try{
            //6 把文档交给IndexWriter
            indexWriter.addDocuments(List.of(doc));
            //7 提交
            indexWriter.commit();
            indexWriter.flush();
        }
        catch ( Exception e)
        {
            e.printStackTrace();
        }

    }

    public void Dispose()
    {
        try{
            if(indexWriter != null)
             //8 关闭
             indexWriter.close();
           if (conf != null)
               conf = null ;
           if(analyzer != null)
               analyzer.close();
            if(directory != null)
                directory.close();
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }

    }

    public Document getDocument()
    {
        //1 创建文档对象
        Document document = new Document();

        return document ;
//        // 创建并添加字段信息。参数：字段的名称、字段的值、是否存储，这里选Store.YES代表存储到文档列表。Store.NO代表不存储
//        document.add(new StringField("id", "1", Field.Store.YES));
//        // 这里我们title字段需要用TextField，即创建索引又会被分词。
//        document.add(new TextField("title", "谷歌地图之父跳槽facebook", Field.Store.YES));
    }
}
