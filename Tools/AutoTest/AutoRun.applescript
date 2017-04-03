
# Activate a destination
activate application "Xcode"
tell application "System Events"
	tell process "Xcode"
		# log title of menu items of menu 1 of menu item "Destination" of menu 1 of menu bar item "Product" of menu bar 1
		click menu item 5 of menu 1 of menu item "Destination" of menu 1 of menu bar item "Product" of menu bar 1
	end tell
	# Click run
	keystroke "r" using command down
end tell