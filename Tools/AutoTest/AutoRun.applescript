#!/usr/bin/osascript
# Activate a destination
# activate application "Xcode"
tell application "System Events"
	tell process "Xcode"
		# set desinationMenu to title of menu items of menu 1 of menu item "Destination" of menu 1 of menu bar item "Product" of menu bar 1
		# log desinationMenu
		# set itemName to name of menu item 6 of menu 1 of menu item "Destination" of menu 1 of menu bar item "Product" of menu bar 1
		# log itemName
		click menu item 6 of menu 1 of menu item "Destination" of menu 1 of menu bar item "Product" of menu bar 1
	end tell
	# Click run
	keystroke "r" using command down
end tell
