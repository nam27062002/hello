Dim objFSO
Dim objShell
Dim thisfolder
Dim allfiles
Dim objExcel

Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")
Set objEnglish	 		= objFSO.CreateTextFile("temp\english.txt", 2, True)
Set objSpanish	 		= objFSO.CreateTextFile("temp\spanish.txt", 2, True)
Set objFrench	 		= objFSO.CreateTextFile("temp\french.txt", 2, True)
Set objBrazilian 		= objFSO.CreateTextFile("temp\brazilian.txt", 2, True)
Set objGerman	 		= objFSO.CreateTextFile("temp\german.txt", 2, True)
Set objItalian	 		= objFSO.CreateTextFile("temp\italian.txt", 2, True)
Set objJapanese	 		= objFSO.CreateTextFile("temp\japanese.txt", 2, True)
Set objKorean	 		= objFSO.CreateTextFile("temp\korean.txt", 2, True)
Set objRussian	 		= objFSO.CreateTextFile("temp\russian.txt", 2, True)
Set objTurkish	 		= objFSO.CreateTextFile("temp\turkish.txt", 2, True)
Set objSimplified 		= objFSO.CreateTextFile("temp\simplified_chinese.txt", 2, True)
Set objTraditional 		= objFSO.CreateTextFile("temp\traditional_chinese.txt", 2, True)
Set thisfolder 			= objFSO.GetFolder("D:\Projects\dragon\Docs\Progression\textExporter")
Set allfiles 			= thisfolder.Files


For Each onefile In allfiles
	j = 10
    If (onefile.type = "Microsoft Excel Macro-Enabled Worksheet") Then
		If FoundStrMatch(onefile.name,"_ENG") = True Then
			language = "ENG"
			j = 8
		End If	
		If FoundStrMatch(onefile.name,"_SPA") = True Then
			language = "SPA"
		End If
		If FoundStrMatch(onefile.name,"_FRE") = True Then
			language = "FRE"
		End If		
		If FoundStrMatch(onefile.name,"_BRA") = True Then
			language = "BRA"
		End If	
		If FoundStrMatch(onefile.name,"_GER") = True Then
			language = "GER"
		End If
		If FoundStrMatch(onefile.name,"_ITA") = True Then
			language = "ITA"
		End If		
		If FoundStrMatch(onefile.name,"_JPN") = True Then
			language = "JPN"
		End If
		If FoundStrMatch(onefile.name,"_KOR") = True Then
			language = "KOR"
		End If	
		If FoundStrMatch(onefile.name,"_RUS") = True Then
			language = "RUS"
		End If	
		If FoundStrMatch(onefile.name,"_TUR") = True Then
			language = "TUR"
		End If		
		If FoundStrMatch(onefile.name,"_SIM") = True Then
			language = "SIM"
		End If	
		If FoundStrMatch(onefile.name,"_TRA") = True Then
			language = "TRA"
		End If		
		Set objExcel = CreateObject("Excel.Application")
		objExcel.DisplayAlerts = False			
		objExcel.Application.Visible = False
        Dim objWorkbook
        Set objWorkbook = objExcel.Workbooks.Open(onefile)
		i = 5
		exitTids = False
		Do Until exitTids
			tid = objWorkbook.Sheets("Menus").Cells(i, 5).Value + "=" + Replace(Replace(Replace(objWorkbook.Sheets("Menus").Cells(i, j  ).Value,Chr(34),""),Chr(10),""),Chr(13),"") + vbCrLf
			If objWorkbook.Sheets("Menus").Cells(i, 5).Value <> "" Then
				If language = "ENG" Then
					objEnglish.Write(tid)
				End If				
				If language = "SPA" Then
					objSpanish.Write(tid)
				End If
				If language = "FRE" Then
					objFrench.Write(tid)
				End If	
				If language = "BRA" Then
					objBrazilian.Write(tid)
				End If
				If language = "GER" Then
					objGerman.Write(tid)
				End If		
				If language = "ITA" Then
					objItalian.Write(tid)
				End If		
				If language = "JPN" Then
					objJapanese.Write(tid)
				End If	
				If language = "KOR" Then
					objKorean.Write(tid)
				End If
				If language = "RUS" Then
					objRussian.Write(tid)
				End If	
				If language = "TUR" Then
					objTurkish.Write(tid)
				End If	
				If language = "SIM" Then
					objSimplified.Write(tid)
				End If	
				If language = "TRA" Then
					objTraditional.Write(tid)
				End If				
			Else
				exitTids = True
			End If
			i = i+1
		Loop 		
    End If
Next
KillProcess()
UTFConvert("english.txt")
UTFConvert("spanish.txt")
UTFConvert("french.txt")
UTFConvert("brazilian.txt")
UTFConvert("german.txt")
UTFConvert("italian.txt")
UTFConvert("japanese.txt")
UTFConvert("korean.txt")
UTFConvert("russian.txt")
UTFConvert("turkish.txt")
UTFConvert("simplified_chinese.txt")
UTFConvert("traditional_chinese.txt")
WScript.echo "Task completed"





Function KillProcess()
   On Error Resume Next 
   Set objWMIService = GetObject("winmgmts:{impersonationLevel=impersonate}" & "!\\.\root\cimv2")
 
   Set colProcess = objWMIService.ExecQuery ("Select * From Win32_Process")
   For Each objProcess in colProcess
      If LCase(objProcess.Name) = LCase("Excel.exe") Then
        objWshShell.Run "TASKKILL /F /T /IM " & objProcess.Name, 0, False
        objProcess.Terminate()
      End If
   Next
End Function

Sub UTFConvert(filename)
	Set fso = CreateObject("Scripting.FileSystemObject")
	txt = fso.OpenTextFile("temp\"+filename, 1, False, -1).ReadAll

	Set stream = CreateObject("ADODB.Stream")
	stream.Open
	stream.Type     = 2 'text
	stream.Position = 0
	stream.Charset  = "utf-8"
	stream.WriteText txt
	stream.SaveToFile "final\"+filename, 2
	stream.Close
End Sub

Function FoundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		FoundStrMatch = true
	Else
		FoundStrMatch = false
	End If
End Function