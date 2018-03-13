import maya.cmds as cmds

pathOfFiles = "C:/Users/dcampos/Documents/Projects/Hungry Dragon 5.6.4/ExportedObj/"
fileType = "obj"

files = cmds.getFileList(folder=pathOfFiles, filespec='*.%s' % fileType)
if len(files) == 0:
    cmds.warning("No files found")
else:
    for f in files:
        cmds.file(pathOfFiles + f, i=True)
        