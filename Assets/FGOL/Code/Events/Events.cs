/* PLEASE NOTE
   If it is needed to add a new value to the enum, please, don't try to rearrange the list and add the new one on the bottom.
   This is because this enumeration is used also in some UI elements, so, changing the position of the values will also change
   the selected value of this enum in the UI where required. (E.G. the quit button in the ingame pause menu).
 	*/
public enum Events
{
    GoInGameButtonPressed,
    OnUserLoggedIn, // receives User class as only param
    OnUserLoggedOut, // receives NULL as params
}