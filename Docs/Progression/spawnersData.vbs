Dim objFSO
Dim MyFile
Dim MyFolder
Dim objShell

Set objFSO 			= CreateObject("Scripting.FileSystemObject")
Set objShell 		= WScript.CreateObject("WScript.Shell")

Set MyFolder 		= objFSO.GetFolder("D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs")
Set objOutputFile 	= objFSO.CreateTextFile("D:\Projects\HungryDragon\Docs\Progression\respawnTimes.txt", 2, true)


For Each MyFile In MyFolder.Files
	If Right(MyFile.Path,7) = ".prefab" Then
		Set objInputFile = objFSO.OpenTextFile(MyFile)
		Dim substrToFind
		Do until objInputFile.AtEndOfStream
			tmpStr = objInputFile.ReadLine
			substrToFind = "m_entityPrefabList:"
			If foundStrMatch(tmpStr,substrToFind) = true Then
				spawner = Replace(MyFile.Path,"D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs\","")
				spawner = Replace(spawner,".prefab","")
				prefab  = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(objInputFile.ReadLine,"- name: ",""),"Surface/",""),"Junk/",""),"Air/",""),"Goblin/",""),"Water/",""),"Monster/",""),"Cage/","")
			End If			
			substrToFind = "m_spawnTime:"
			If foundStrMatch(tmpStr,substrToFind) = true Then
				minTime = Replace(objInputFile.ReadLine,"min:","")
				maxTime = Replace(objInputFile.ReadLine,"max:","")
				text = spawner + ";" + minTime + ";" + maxTime + ";" + prefab + vbCrLf
				objOutputFile.Write(text)
			End If
		Loop
	End If
Next

Function foundStrMatch(tmpStr,substrToFind)
	If InStr(tmpStr, substrToFind) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function