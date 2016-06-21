##########################################################
##
## CALETY BARCELONA
## INITIALISER
##
##########################################################

#!/bin/bash

echo
echo "CALETY INITIALISER"
echo "Preparing Calety framework and symbolic linkages ..."
echo

createSymbolicLinks ()
{
    echo "Generating symbolic linkages to Calety ..."
    echo

    strCurrentFolder=$(pwd)

    cd ./../../calety
    git checkout develop
    git pull
    cd $strCurrentFolder

    cd Assets

    strPathToCaletyCS=./../../../calety/Calety/UnityProject/Assets/Calety

    if [ ! -L "Calety" ]; then
        ln -s $strPathToCaletyCS
    fi

    cd Editor

    strPathToCaletyEditorCS=./../../../../calety/Calety/UnityProject/Assets/Editor/Calety

    if [ ! -L "Calety" ]; then
        ln -s $strPathToCaletyEditorCS
    fi

    cd ..

    if [ ! -d "CaletyExternalPlugins" ]; then
        mkdir CaletyExternalPlugins

        cd CaletyExternalPlugins

        echo "Plugins" > ".gitignore"

        cd ..
    fi

    cd CaletyExternalPlugins

    strPathToCaletyPlugins=./../../../../calety/Calety/UnityProject/Assets/CaletyExternalPlugins/Plugins

    if [ ! -L "Plugins" ]; then
        ln -s $strPathToCaletyPlugins Plugins
    fi

    cd ..
    cd ..

    echo "Done. Thanks for using Calety."
    echo
}

searchForCalety ()
{
    strPrefixToFind="/calety"
    strPathToCaletySDK="."
    strPathToSearch="${strPathToCaletySDK}${strPrefixToFind}"
    maxTries=10

    for ((x=0; x<=maxTries; x++)); do
        strPathToCaletySDK="${strPathToCaletySDK}/.."
        strPathToSearch="${strPathToCaletySDK}${strPrefixToFind}"

        if [ -e "${strPathToSearch}" ]
        then
            createSymbolicLinks

            break
        fi

        if [ $x = $maxTries ]
        then
            echo "No Calety was found. Now GIT is going to checkout the Calety framework. This could last some minutes. Please wait..."
            echo

            git clone git@bcn-mb-git.ubisoft.org:tools/calety.git ./../../calety

            createSymbolicLinks
        fi
    done
}

searchForCalety
