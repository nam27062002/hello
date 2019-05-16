##########################################################
##
## UBIPACKAGES
## INITIALIZER
##
##########################################################

echo
echo "Preparing UbiPackages framework and symbolic linkages..."
echo

link()
{
	echo "link" $1 $2

	folderName=$2

	#Path to the original directory has to be absolute 
	pathToFolderOriginal=$PWD"/../Assets/"$folderName
	pathToFolderLink="$1/Assets/"$folderName

	# Checks if the link already exists 
	if [ -L $pathToFolderLink ]
	then
		echo "Symbolic link ${pathToFolderLink} already exists"
	else
		createSymbolicLink $pathToFolderOriginal $pathToFolderLink	
	fi
}

createSymbolicLink()
{
	echo "Symbolic linking $1 to $2..."
	ln -s $1 $2

	if [ -L $2 ]
	then
		echo "SUCCESS"
	fi
}

project()
{
	link $1 UbiPackages
	#link $1 Calety
	#link $1 CaletyExternalPlugins

	#path=$1"/Assets/Plugins"
	#mkdir $path
	#mkdir $path"/iOS"
	#link $1 Plugins/iOS/Calety
}

project AddressablesUnitTests
project AddressablesAssetsTests