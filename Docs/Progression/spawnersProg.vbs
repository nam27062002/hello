Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set objOutputFile 		= objFSO.CreateTextFile("spawnersProg.txt", 2, true)


REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle_Market.unity")
REM Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle_Mines.unity")
Set objInputFile = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Dark.unity")
Dim substrToFind

	Do until objInputFile.AtEndOfStream
		substrToFind = "propertyPath: m_Name"
		tmpStr = objInputFile.ReadLine
		If foundStrMatch(tmpStr,substrToFind) = true Then
			tmpStr = objInputFile.ReadLine
			REM WScript.echo tmpStr
			tmpStrToSave = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP@"),"_Evade","@Evade"),"_Generic","@Generic"),"_Squid","@Squid"),"_Static","@Static"),"_Flock","@Flock"),"_Dark","@Dark"),"_Mix","@Mix"),"_Random","@Random"),"_Root","@Root")," ",""),"Air_","Air@"),"PF_","PF@"),"BG_","BG@"),"_Path","@Path"),"Star_H","Star@H"),"Star_U","Star@U"),"Star_N","Star@N"),"Star_G","Star@G"),"Star_R","Star@R"),"Star_Y","Star@Y"),"_Plant","@Plant"),"_",";")
			tmpStr = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(tmpStr,"value: ",""),"SP_","SP"),"_Evade",""),"_Generic",""),"_Squid",""),"_Static",""),"_Flock",""),"_Dark",""),"_Mix",""),"_Random",""),"_Root","")," ",""),"PF_","PF"),"BG_",""),"_Path",""),"_Plant",""),"Air_",""),"Star_H","Star"),"Star_U","Star"),"Star_N","Star"),"Star_G","Star"),"Star_R","Star"),"Star_Y","Star"),"_",";")
			tmpStr2 = ";"
			If foundStrMatch(tmpStr,tmpStr2) = false Then
				tmpStr = Replace(tmpStrToSave,"@","_") + ";0;0;-"
			Else
				tmpStr = Replace(tmpStrToSave,"@","_")
			End If
			tmpStr3 = ";XP"
			tmpStr4 = ";TIME"
			tmpStr5 = ";KILL"
			If foundStrMatch(tmpStr,tmpStr3) = true Then
				tmpStr = Replace(tmpStr,tmpStr3,"")
				tmpStr = tmpStr + tmpStr3
			End If
			If foundStrMatch(tmpStr,tmpStr4) = true Then
				tmpStr = Replace(tmpStr,tmpStr4,"")
				tmpStr = tmpStr + tmpStr4
			End If
			
			If foundStrMatch(tmpStr,tmpStr5) = true Then
				tmpStr = Replace(tmpStr,tmpStr5,"")
				tmpStr = tmpStr + tmpStr5
			End If			
			
			tmpStr = tmpStr + vbCrLf
			REM inactive spawners ends with "-IN" (check the 'rename' script in Unity)
			If foundStrMatch(tmpStr,"SP_BG") = false And foundStrMatch(tmpStr,"SP_Seasonal") = false  And foundStrMatch(tmpStr,"SP_FlyingTicket") = false  And foundStrMatch(tmpStr,"-IN") = false  And foundStrMatch(tmpStr,"SP_") = true Then
				objOutputFile.Write(tmpStr)
			End If
		End If
	Loop

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function