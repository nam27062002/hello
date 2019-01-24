##########################################################
##
## UBIPACKAGES
## INITIALIZER
##
##########################################################

echo
echo "Preparing UbiPackages framework and symbolic linkages..."
echo

link_ubi_packages()
{
	echo "link_ubi_packages" $1

	ubiPackagesFolderName="UbiPackages"

	#Path to the original directory has to be absolute 
	pathToUbiPackagesOriginal=$PWD"/../Assets/"$ubiPackagesFolderName
	pathToUbiPackagesLink="$1/Assets/"$ubiPackagesFolderName

	#echo "pathToUbiPackagesOriginal="$pathToUbiPackagesOriginal
	#echo "pathToUbiPackagesLink="$pathToUbiPackagesLink

	# Checks if the link already exists 
	if [ -L $pathToUbiPackagesLink ]
	then
		echo "Symbolic link ${pathToUbiPackagesLink} already exists"
	else
		createSymbolicLink $pathToUbiPackagesOriginal $pathToUbiPackagesLink	
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

link_ubi_packages AssetBundlesUnitTests