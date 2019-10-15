Dim objFSO
Dim objShell
Dim thisfolder
Dim allfiles
Dim objExcel

Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")
Set objOutputFile 		= objFSO.CreateTextFile("temp\spanish.txt", 2, True)
Set thisfolder 			= objFSO.GetFolder("D:\Projects\dragon\Docs\Progression\textExporter")
Set allfiles 			= thisfolder.Files


For Each onefile In allfiles
    If (onefile.type = "Microsoft Excel Macro-Enabled Worksheet") Then
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
				objOutputFile.Write(tid)
			Else
				exitTids = True
			End If
			i = i+1
		Loop 		
    End If
Next
KillProcess()
UTFConvert("temp\spanish.txt")
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
	txt = fso.OpenTextFile(filename, 1, False, -1).ReadAll

	Set stream = CreateObject("ADODB.Stream")
	stream.Open
	stream.Type     = 2 'text
	stream.Position = 0
	stream.Charset  = "utf-8"
	stream.WriteText txt
	stream.SaveToFile "final\spanish.txt", 2
	stream.Close
End Sub