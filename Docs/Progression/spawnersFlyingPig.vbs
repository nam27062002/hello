Dim objFSO
Dim objShell



Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set objOutputFile 		= objFSO.CreateTextFile("spawnersFlyingPig.txt", 2, true)


Set objInputFileVillage = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village.unity")
Set objInputFileVillageForest = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Village_Forest.unity")
Set objInputFileCastleMarket = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle_Market.unity")
Set objInputFileCastleMines = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Castle_Mines.unity")
Set objInputFileDark = objFSO.OpenTextFile("..\..\Assets\Game\Scenes\Levels\Spawners\SP_Medieval_Final_Dark.unity")
Dim CountPigs
	REM SCENE 1: VILLAGE + FOREST
	CountPigs = 0
	Do until objInputFileVillage.AtEndOfStream
		tmpStr = objInputFileVillage.ReadLine
		If foundStrMatch2(tmpStr,"SP_FlyingPig") = true Then
			CountPigs = CountPigs + 1
		End if	
	Loop
	Do until objInputFileVillageForest.AtEndOfStream
		tmpStr = objInputFileVillageForest.ReadLine
		If foundStrMatch2(tmpStr,"SP_FlyingPig") = true Then
			CountPigs = CountPigs + 1
		End if	
	Loop	
	objOutputFile.Write(Cstr(CountPigs) + vbCrLf)
	REM SCENE 2: CASTLE + MINES
	CountPigs = 0
	Do until objInputFileCastleMarket.AtEndOfStream
		tmpStr = objInputFileCastleMarket.ReadLine
		If foundStrMatch2(tmpStr,"SP_FlyingPig") = true Then
			CountPigs = CountPigs + 1
		End if	
	Loop
	Do until objInputFileCastleMines.AtEndOfStream
		tmpStr = objInputFileCastleMines.ReadLine
		If foundStrMatch2(tmpStr,"SP_FlyingPig") = true Then
			CountPigs = CountPigs + 1
		End if	
	Loop	
	objOutputFile.Write(Cstr(CountPigs) + vbCrLf)
	REM SCENE 3: DARK
	CountPigs = 0
	Do until objInputFileDark.AtEndOfStream
		tmpStr = objInputFileDark.ReadLine
		If foundStrMatch2(tmpStr,"SP_FlyingPig") = true Then
			CountPigs = CountPigs + 1
		End if	
	Loop
	objOutputFile.Write(Cstr(CountPigs) + vbCrLf)	

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 And Len(tmpStr) = 30 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function

Function foundStrMatch2(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch2 = true
	Else
		foundStrMatch2 = false
	End If
End Function