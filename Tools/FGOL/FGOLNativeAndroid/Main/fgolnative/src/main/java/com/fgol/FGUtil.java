package com.fgol;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.nio.channels.FileChannel;
import java.io.IOException;


public class FGUtil 
{
	public static void CopyFilesRecursively(File src, File dest) throws IOException
    {
    	if(src.isDirectory())
    	{
    		//if directory not exists, create it
    		if(!dest.exists())
    		{
    		   dest.mkdir();
    		   System.out.println("Directory copied from " 
                              + src + "  to " + dest);
    		}
    		
    		//list all the directory contents
    		String files[] = src.list();
    		
    		for (int i=0; i< files.length; i++) 
    		{
    		   //construct the src and dest file structure
    		   File srcFile = new File(src, files[i]);
    		   File destFile = new File(dest, files[i]);
    		   //recursive copy
    		   CopyFilesRecursively(srcFile,destFile);
    		}
    	   
    	}
    	else
    	{
    		FileInputStream inStream = new FileInputStream(src);
    	    FileOutputStream outStream = new FileOutputStream(dest);
    	    FileChannel inChannel = inStream.getChannel();
    	    FileChannel outChannel = outStream.getChannel();
    	    inChannel.transferTo(0, inChannel.size(), outChannel);
    	    inStream.close();
    	    outStream.close();
	        System.out.println("File copied from " + src + " to " + dest);
    	}
    }
}
