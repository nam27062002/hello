Dim objFSO
Dim objShell
Dim header, icolumn, startHeader, endHeader
Dim tables2Find, table2Find, table2FindRow, table2FindColumn
Dim tables2FindArray
Dim WSCount, ws

Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

REM TABLES TO FIND
tables2Find				= "{dragonTierDefinitions};{dragonDefinitions};{dragonSettings};{dragonHealthModifiersDefinitions};{dragonProgressionDefinitions}"
tables2FindArray 		= Split(tables2Find, ";")

Set obj 				= createobject("Excel.Application")   
obj.visible 			= False                                    
Set objExcel 			= obj.Workbooks.open(objFSO.GetAbsolutePathName(".") + "\HungryDragonContent_Dragons.xlsx") 



WSCount = objExcel.Worksheets.Count
REM CHECK EACH SHEET IN THE EXCEL
For ws = 1 To WSCount
	REM CHECK EACH TABLE
	For table2Find = 0 To UBound(tables2FindArray)
		Set foundTable = objExcel.Worksheets(ws).Range("A1:C500").Find(tables2FindArray(table2Find))
		If Not FoundTable Is Nothing Then
			table2FindRow = foundTable.Row
			table2FindColumn = foundTable.Column

			startHeader = table2FindColumn + 1
			endHeader = table2FindColumn + 100
			REM CHECK EACH COLUMN HEADER OF CURRENT TABLE
			For icolumn = startHeader To endHeader
				header = objExcel.Worksheets(ws).Cells(table2FindRow,icolumn).Value 
				If Not IsEmpty(header) Then
					REM TODO: Check if that column should be added to the json,and do it!!! [column_name]CRM (if the colum ends with "CRM" then we add it
					If InStr(header, "CRM") > 0 Then
						MsgBox (header)
					End If
				End If
			Next 
		Else
			MsgBox ("Table: " & table2Find & " not found")
		End If
	Next
Next
   
objExcel.Close
obj.Quit



