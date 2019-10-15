Dim objFSO
Dim objShell
Dim thisfolder
Dim allfiles
Dim objExcel

Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")
Set objSpanish	 		= objFSO.CreateTextFile("temp\spanish.txt", 2, True)
Set objFrench	 		= objFSO.CreateTextFile("temp\french.txt", 2, True)
Set thisfolder 			= objFSO.GetFolder("D:\Projects\dragon\Docs\Progression\textExporter")
Set allfiles 			= thisfolder.Files


For Each onefile In allfiles
    If (onefile.type = "Microsoft Excel Macro-Enabled Worksheet") Then
		If FoundStrMatch(onefile.name,"_SPA") = True Then
			language = "SPA"
		End If
		If FoundStrMatch(onefile.name,"_FRE") = True Then
			language = "FRE"
		End If		
		Set objExcel = CreateObject("Excel.Application")
		objExcel.DisplayAlerts = False			
		objExcel.Application.Visible = False
        Dim objWorkbook
        Set objWorkbook = objExcel.Workbooks.Open(onefile)
		i = 5
		exitTids = False
		Do Until exitTids
			tid = objWorkbook.Sheets("Menus").Cells(i, 5).Value + "=" + Replace(Replace(Replace(objWorkbook.Sheets("Menus").Cells(i, 10  ).Value,Chr(34),""),Chr(10),""),Chr(13),"") + vbCrLf
			If objWorkbook.Sheets("Menus").Cells(i, 5).Value <> "" Then
				If language = "SPA" Then
					objSpanish.Write(tid)
				End If
				If language = "FRE" Then
					objFrench.Write(tid)
				End If				
			Else
				exitTids = True
			End If
			i = i+1
		Loop 		
    End If
Next
KillProcess()
UTFConvert("spanish.txt")
UTFConvert("french.txt")
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