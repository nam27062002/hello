Dim objFSO
Dim CurrentSpawner
Dim SpawnersFolder
Dim objShell

Public spawner
Public prefab
Public prefabFile


Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

Set SpawnersFolder 		= objFSO.GetFolder("D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs")
Public EntityFile 
Set objOutputFile 		= objFSO.CreateTextFile("D:\Projects\HungryDragon\Docs\Progression\spawnersData.txt", 2, true)


For Each CurrentSpawner In SpawnersFolder.Files
	If Right(CurrentSpawner.Path,7) = ".prefab" Then
		Set objInputFile = objFSO.OpenTextFile(CurrentSpawner)
		Dim substrToFind
		Do until objInputFile.AtEndOfStream
			currentSpawnerInfo = spawnerInfo()
			If currentSpawnerInfo <> "" Then
				contentSku = entityInfo()
				If contentSku <> "" Then
					currentContentInfo = contentInfo(contentSku)
					currentSpawnerInfo = Replace(contentSku + ";" + currentSpawnerInfo + ";" + currentContentInfo + vbCrLf," ","")
				End If
				objOutputFile.Write(currentSpawnerInfo)
			End If
		Loop
	End If
Next


Function spawnerInfo()
	tmpStr = objInputFile.ReadLine
	substrToFind = "m_entityPrefabList:"
	If foundStrMatch(tmpStr,substrToFind) = true Then
		spawner = Replace(CurrentSpawner.Path,"D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs\","")
		spawner = Replace(spawner,".prefab","")
		prefabFolder = Replace(objInputFile.ReadLine,"- name: ","")
		aux = Replace(Replace(prefabFolder,"/","\")," ","")
		prefabFile = "D:\Projects\HungryDragon\Assets\Resources\Game\Entities\NewEntites\"+ aux + ".prefab"
		prefab  = Replace(Replace(Replace(Replace(Replace(Replace(Replace(prefabFolder,"Surface/",""),"Junk/",""),"Air/",""),"Goblin/",""),"Water/",""),"Monster/",""),"Cage/","")
	End If			
	substrToFind = "m_spawnTime:"
	If foundStrMatch(tmpStr,substrToFind) = true Then
		minTime = Replace(objInputFile.ReadLine,"min:","")
		maxTime = Replace(objInputFile.ReadLine,"max:","")
		spawnerInfo = spawner + ";" + prefab + ";" + minTime + ";" + maxTime
	Else
		spawnerInfo = ""
	End If
End Function

Function entityInfo()
	Set EntityFile = objFSO.OpenTextFile(prefabFile)
	substrToFind = "m_sku:"
	entityInfo = ""
	Do until EntityFile.AtEndOfStream
		tmpStr = EntityFile.ReadLine
		If foundStrMatch(tmpStr,substrToFind) = true Then
			entityInfo = Replace(Replace(tmpStr,"m_sku:","")," ","")
		End If
	Loop
End Function

Function contentInfo(contentSku)
	Set ContentFile	= objFSO.OpenTextFile("D:\Projects\HungryDragon\Assets\Resources\Rules\entityDefinitions.xml")
	Do until ContentFile.AtEndOfStream
		tmpStr = ContentFile.ReadLine
		If foundStrMatch(tmpStr,contentSku) = true Then
			pos = InStr(tmpStr, "rewardHealth=")
			rewardhp = Replace(Replace(Replace(Mid(tmpStr,pos+13,4),Chr(34),"")," ",""),"r","")
			pos = InStr(tmpStr, "rewardXp=")
			rewardxp = Replace(Replace(Replace(Mid(tmpStr,pos+10,4),Chr(34),"")," ",""),"r","")
			contentInfo = rewardhp + ";" + rewardxp
		End If
	Loop
End function

Function foundStrMatch(tmpStr,substrToFind)
	If InStr(tmpStr, substrToFind) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function