Dim objFSO
Dim CurrentSpawner
Dim SpawnersFolder
Dim objShell

Public spawner
Public prefab
Public prefabFile
Public damage


Set objFSO 				= CreateObject("Scripting.FileSystemObject")
Set objShell 			= WScript.CreateObject("WScript.Shell")

REM Set SpawnersFolder 		= objFSO.GetFolder("D:\Projects\HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs")
Set SpawnersFolder 		= objFSO.GetFolder("..\..\Assets\Tools\LevelEditor\SpawnerPrefabs")
Public EntityFile 
REM Set objOutputFile 		= objFSO.CreateTextFile("D:\Projects\HungryDragon\Docs\Progression\spawnersData.txt", 2, true)
Set objOutputFile 		= objFSO.CreateTextFile("spawnersData.txt", 2, true)


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
					currentSpawnerInfo = Replace(Replace(currentSpawnerInfo,"contentSku",contentSku) + ";" + currentContentInfo + ";" + damage + vbCrLf," ","")
				Else
					currentSpawnerInfo = Replace(Replace(currentSpawnerInfo,"contentSku","-") + ";" + damage + vbCrLf," ","")
				End If
				objOutputFile.Write(currentSpawnerInfo)
			End If
		Loop
	End If
Next


Function spawnerInfo()
	tmpStr = objInputFile.ReadLine
	substrToFind = "m_entityPrefabList:"
	substrToFind2 = "m_entityPrefab:"
	If foundStrMatch(tmpStr,substrToFind) = true Or foundStrMatch(tmpStr,substrToFind2) = true Then
		spawner = Replace(CurrentSpawner.Path,"D:\Projects\dragon\client\Assets\Tools\LevelEditor\SpawnerPrefabs\","")										   
		REM spawner = Replace(CurrentSpawner.Path,"HungryDragon\Assets\Tools\LevelEditor\SpawnerPrefabs\","")
		spawner = Replace(spawner,".prefab","")
		If foundStrMatch(tmpStr,substrToFind) = true Then
			prefabFolder = Replace(objInputFile.ReadLine,"- name: ","")
		Else
			prefabFolder = Replace(tmpStr,"m_entityPrefab: ","")
		End If
		aux = Replace(Replace(prefabFolder,"/","\")," ","")
		REM prefabFile = "D:\Projects\HungryDragon\Assets\Resources\Game\Entities\NewEntites\"+ aux + ".prefab"
		prefabFile = "..\..\Assets\Resources\Game\Entities\NewEntites\"+ aux + ".prefab"
		prefab  = Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(Replace(prefabFolder,"Surface/",""),"Junk/",""),"Air/",""),"Goblin/",""),"Water/",""),"Monster/",""),"Cage/",""),"Vehicles/",""),"Magic/",""),"Dragon/","")
	End If			
	substrToFind = "m_spawnTime:"
	If foundStrMatch(tmpStr,substrToFind) = true Then
		minTime = Replace(objInputFile.ReadLine,"min:","")
		maxTime = Replace(objInputFile.ReadLine,"max:","")
		spawnerInfo = spawner + ";contentSku;" + prefab + ";" + minTime + ";" + maxTime
	Else
		spawnerInfo = ""
	End If
End Function

Function entityInfo()
	On Error Resume Next
	Set EntityFile = objFSO.OpenTextFile(prefabFile)
	If Err.Number <> 0 Then
		WScript.Echo "Error: " & prefabFile
		Err.Clear
	End If
	substrToFind = "m_sku:"
	substrToFind_1 = "damage"+Chr(34)
	entityInfo = ""
	damage = "-"
	content_sku = ""
	Do until EntityFile.AtEndOfStream
		tmpStr = EntityFile.ReadLine
		If foundStrMatch(tmpStr,substrToFind_1) = true Then	
			pos = InStr(tmpStr, substrToFind_1)
			damage = Replace(Replace(Replace(Mid(tmpStr,pos+8,3),Chr(34),"")," ",""),",","")			
		End If		
		If foundStrMatch(tmpStr,substrToFind) = true Then
			content_sku = Replace(Replace(tmpStr,"m_sku:","")," ","")
		End If
	Loop	
	entityInfo = content_sku
End Function

Function contentInfo(contentSku)
	REM Set ContentFile	= objFSO.OpenTextFile("D:\Projects\HungryDragon\Assets\Resources\Rules\entityDefinitions.xml")
	Set ContentFile	= objFSO.OpenTextFile("..\..\Assets\Resources\Rules\entityDefinitions.xml")
	continueSearch = true
	Do until ContentFile.AtEndOfStream
		tmpStr = ContentFile.ReadLine
		If foundStrMatch(tmpStr,contentSku) = true And continueSearch = true Then
			continueSearch = false
			pos = InStr(tmpStr, "rewardHealth=")
			rewardhp = Replace(Replace(Replace(Mid(tmpStr,pos+13,4),Chr(34),"")," ",""),"r","")
			pos = InStr(tmpStr, "rewardXp=")
			rewardxp = Replace(Replace(Replace(Mid(tmpStr,pos+10,4),Chr(34),"")," ",""),"r","")
			pos = InStr(tmpStr, "latchOnFromTier=")
			latchTier = Mid(tmpStr, pos+17,1)
			pos = InStr(tmpStr, "grabFromTier=")
			grabTier = Mid(tmpStr, pos+14,1)
			pos = InStr(tmpStr, "edibleFromTier=")
			edibleTier = Mid(tmpStr, pos+16,1)	
			edible = Min(latchTier, grabTier, edibleTier)
			pos = InStr(tmpStr,"burnableFromTier=")
			burnableTier = Mid(tmpStr, pos+18,1)
			contentInfo = rewardhp + ";" + rewardxp + ";" + edible + ";" + burnableTier
		End If
	Loop
End function

Function foundStrMatch(tmpStr,substrToFind_2)
	If InStr(tmpStr, substrToFind_2) > 0 Then
		foundStrMatch = true
	Else
		foundStrMatch = false
	End If
End Function

Function Min(a,b,c)
    aux = a
    If b < aux Then 
		aux = b
	End If
	If c < aux Then 
		Min = c
	Else 
		Min = aux
	End If
End Function 