import maya.cmds as cmds

pathOfFiles = "C:/Users/dcampos/Documents/Projects/Hungry Dragon 5.6.4/ExportedObj/"
#pathOfFiles = "C:/Users/dcampos/Documents/Projects/"
fileType = "obj"

# pathOfFiles = cmds.fileDialog2(dialogStyle=2, startingDirectory=pathOfFiles, fileMode=3)

print 'Path:', pathOfFiles

files = cmds.getFileList(folder=pathOfFiles, filespec='*.%s' % fileType)
if len(files) == 0:
    cmds.warning("No files found")
else:
    for f in files:
        cmds.file(pathOfFiles + f, i=True)
        