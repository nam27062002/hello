package com.fgol;

import java.net.*;
import java.io.*;

import java.util.Date;
import java.util.Locale;
import java.text.DateFormat;

import android.os.AsyncTask;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.SharedPreferences;

public class FGN
{
	String FGN_BASE_URL = "http://www.futuregamesnetwork.mobi/";
	String kFGNMessageDateKey = "FGN-Message-Date";

	static void FGN_LOG(String msg) {
		//System.out.println(msg);
	}

	static String sLink="";
	static String sDate="";
	static boolean alreadyShown = false;

	public FGN(String filename)
	{
		if (!alreadyShown) {
			Download(FGN_BASE_URL + filename);
		}
	}

	public void Download(String fileURI)
	{
		FGN_LOG("FGNDownload " + fileURI);

		URL fileURL;
		try  {
			fileURL = new URL(fileURI);
			new DownloadFileTask().execute(fileURL);
		}
		catch (MalformedURLException e) {
			FGN_LOG(e.toString());
		}
		catch (Exception e) {
			FGN_LOG(e.toString());
		}
	}

	// Extension of AsyncTask<Params,Progress,Result> with:
	// Params - URL
	// Progress - Integer
	// Result - byte[]
	private class DownloadFileTask extends AsyncTask<URL, Integer, byte[]>
	{
		byte[] fileData = null;

		protected void downloadHTTP(URL url) throws IOException
		{
			URLConnection connection = (URLConnection) url.openConnection();

			InputStream is = connection.getInputStream();
			BufferedReader in = new BufferedReader(new InputStreamReader(is));
			String inputLine;
			String message="";

			while ((inputLine = in.readLine()) != null) {
				System.out.println(inputLine);
				message += inputLine + "\n";
			}

			fileData = message.getBytes();

			in.close();
		}

		// Override of Asynctask.doInBackground(Object... objects)
		protected byte[] doInBackground(URL... paths)
		{
			URL url;

			try
			{
				url = paths[0];
			    String protocol = url.getProtocol();
			    if(protocol.equalsIgnoreCase("http"))
			    {
			    	// ensure we haven't passed in an ftp link...
			    	downloadHTTP(url);

					return fileData;
			    }
			}
			catch (MalformedURLException e)
			{
				FGN_LOG("Malformed exception: " + e.toString());
			}
			catch (IOException e)
			{
				FGN_LOG("IOException: " + e.toString());
			}
			catch (Exception e)
			{
				FGN_LOG("Exception: " + e.toString());
			}
			return null;
		}

		// Override of Asynctask.onPostExecute(Object result)
		protected void onPostExecute(byte[] result)
		{
			if (result==null) {
				FGN_LOG("No FGN message data returned.");
				return;
			}
			
			try {
				String output = new String(result);
				FGN_LOG("Connected to FGN. Message = " + output);

				final Activity activity = FGOLNative.activity;
				
				String[] lines = output.split("\n");
				if (lines==null) return;
				
				for (int i=0; i<lines.length; i++)
				{
					String line = lines[i];
					FGN_LOG("line " + i + " = '" + line + "'\n");
				}
				
				String header = lines[0];
				
				if (header.equals("FGN-MESSAGE") && lines.length >= 4)
				{
					// read message data
					String title = lines[1];
					String message = lines[2];
					sLink = lines[3];
					sDate = lines[4];

					// read stored date
					SharedPreferences prefs = activity.getPreferences(Context.MODE_PRIVATE);
					String lastDate = prefs.getString(kFGNMessageDateKey, "01/01/2000");
					if (lastDate!=null) {

						FGN_LOG("fgnWebsiteThread - last date " + lastDate);

						if ( lastDate.equals(sDate) ) {
							alreadyShown = true;
							FGN_LOG("fgnWebsiteThread - Already shown this message");
						} else { // compare the dates

							try {
								DateFormat dateFormatter = DateFormat.getDateInstance(DateFormat.SHORT, Locale.UK);
								Date dateStored = dateFormatter.parse(lastDate);
								Date dateRead = dateFormatter.parse(sDate);

								boolean date_newer = dateRead.after(dateStored);
								FGN_LOG("fgnWebsiteThread - date is newer? " + (date_newer ? "YES" : "NO") );

								alreadyShown = !date_newer;
							} catch (Exception e) {
								FGN_LOG("Exception: " + e.toString() + "\nlastdate="+lastDate+" readdate="+sDate);
							}
						}
					}

					FGN_LOG("fgnWebsiteThread - storing date " + sDate);
					SharedPreferences.Editor editor = prefs.edit();
					editor.putString(kFGNMessageDateKey, sDate);
					editor.commit();

					if (!alreadyShown) {
				        AlertDialog.Builder builder = new AlertDialog.Builder(activity);
						builder.setTitle(title);
				        builder.setMessage(message);
				        builder.setCancelable(false)
				          .setPositiveButton("Show Me", new DialogInterface.OnClickListener() {
				           public void onClick(DialogInterface dialog, int id) {
							   FGOLNative.openURL(sLink);
							   dialog.cancel();
				           }
				       })
				       .setNegativeButton("Dismiss", new DialogInterface.OnClickListener() {
				           public void onClick(DialogInterface dialog, int id) {
				               dialog.cancel();
				           }
				       });
				        AlertDialog alertdialog = builder.create();
				        alertdialog.show();
						alreadyShown = true;
						return;
					}

				} else {
					FGN_LOG("fgnWebsiteThread - Invalid Header\n");
				}
			
			} catch (Exception e) {
				System.out.println("Exception in FGN message download: " + e.toString());
				e.printStackTrace();
			}
		}

		// Override of AsyncTask.onProgressUpdate(Integer... progress)
		protected void onProgressUpdate(Integer... progress)
		{
			FGN_LOG("Download progress: " + progress[0]);
		}

	}
}
