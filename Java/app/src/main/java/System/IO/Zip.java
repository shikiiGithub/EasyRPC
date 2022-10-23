package System.IO;

/**
 * Created by shikii on 2018/2/22.
 */
/**
 * 学习使用java.util.zip压缩文件或者文件夹
 * @author lhm
 *
 */
        import java.io.*;
        import java.io.File;
        import java.util.Enumeration;
        import java.util.zip.ZipEntry;
        import java.util.zip.ZipFile;
        import java.util.zip.ZipOutputStream;



public class Zip {


    //主要使用这个方法
    /**
     * 按照原路径的类型就行压缩。文件路径直接把文件压缩，
     * @param src
     * @param zos
     * @param baseDir
     */
    private static void CompressbyType(File src, ZipOutputStream zos,String baseDir) {

        if (!src.exists())
            return;
        System.out.println("压缩路径" + baseDir + src.getName());
        //判断文件是否是文件，如果是文件调用compressFile方法,如果是路径，则调用compressDir方法；
        if (src.isFile()) {
            //src是文件，调用此方法
            CompressFile(src, zos, baseDir);

        } else if (src.isDirectory()) {
            //src是文件夹，调用此方法
            CompressDir(src, zos, baseDir);

        }

    }

    /**s
     * 压缩文件
     * @param srcFilePath 压缩源路径
     * @param destFilePath 压缩目的路径
     */
    public static void Compress(String srcFilePath, String destFilePath) {
        //
        File src = new File(srcFilePath);

        if (!src.exists()) {
            throw new RuntimeException(srcFilePath + "不存在");
        }
        File zipFile = new File(destFilePath);

        try {

            FileOutputStream fos = new FileOutputStream(zipFile);
            ZipOutputStream zos = new ZipOutputStream(fos);
            String baseDir = "";
            CompressbyType(src, zos, baseDir);
            zos.close();

        } catch (Exception e) {
            // TODO Auto-generated catch block
            e.printStackTrace();

        }
    }

    /**
     * 压缩文件
     */
    private static void CompressFile(File file, ZipOutputStream zos, String baseDir) {
        if (!file.exists())
            return;
        try {
            BufferedInputStream bis = new BufferedInputStream(new FileInputStream(file));
            ZipEntry entry = new ZipEntry(baseDir + file.getName());
            zos.putNextEntry(entry);
            int count;
            byte[] buf = new byte[1024];
            while ((count = bis.read(buf)) != -1) {
                zos.write(buf, 0, count);
            }
            bis.close();

        } catch (Exception e) {
            // TODO: handle exception

        }
    }

    /**
     * 压缩文件夹
     * 不能操作中文路径
     */
    private static void CompressDir(File dir, ZipOutputStream zos, String baseDir) {
        if (!dir.exists())
            return;
        File[] files = dir.listFiles();
        if(files.length == 0){
            try {
                zos.putNextEntry(new ZipEntry(baseDir + dir.getName()+File.separator));
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
        for (File file : files) {
            CompressbyType(file, zos, baseDir + dir.getName() + File.separator);
        }
    }

    public static void decompress(String srcPath, String dest)  {

        File file = new File(srcPath);

        if (!file.exists()) {

            throw new RuntimeException(srcPath + "所指文件不存在");

        }

        ZipFile zf = null;
        try {
            zf = new ZipFile(file);


        Enumeration entries = zf.entries();

        ZipEntry entry = null;

        while (entries.hasMoreElements()) {

            entry = (ZipEntry) entries.nextElement();

            System.out.println("解压" + entry.getName());

            if (entry.isDirectory()) {

                String dirPath = dest + File.separator + entry.getName();

                File dir = new File(dirPath);

                dir.mkdirs();

            } else {

                // 表示文件

                File f = new File(dest + File.separator + entry.getName());

                if (!f.exists()) {



                    File parentDir = f.getParentFile() ;

                    parentDir.mkdirs();



                }

                f.createNewFile();

                // 将压缩文件内容写入到这个文件中

                InputStream is = zf.getInputStream(entry);

                FileOutputStream fos = new FileOutputStream(f);



                int count;

                byte[] buf = new byte[8192];

                while ((count = is.read(buf)) != -1) {

                    fos.write(buf, 0, count);

                }

                is.close();

                fos.close();

            }

        }
        } catch (IOException e) {
            e.printStackTrace();
        }

    }
}