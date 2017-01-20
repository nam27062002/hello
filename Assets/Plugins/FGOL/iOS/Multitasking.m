// Multitasking Features --------------------------------------------------------
#import <AVFoundation/AVFoundation.h>
#import <AVKit/AVKit.h>

//  Extern functions
extern bool _IsIPodMusicPlaying();

//  Returns major OS version
int _GetOSVersion()
{
    return (int) [[NSProcessInfo processInfo] operatingSystemVersion].majorVersion;
}

//  Returns if OS version is equal majorVersion, set includeHigher to check if equal or higher
bool _IsOperatingSystemOfVersion(unsigned int majorVersion, bool includeHigher)
{
    if (includeHigher)
        return _GetOSVersion() >= majorVersion;
    else
        return _GetOSVersion() == majorVersion;
}

//  Checks if audio is playing from other Apps
bool _IsAudioPlayingFromOtherApps()
{
    bool audioIsPlaying = false;
    if (_IsOperatingSystemOfVersion(8, true))
    {
        audioIsPlaying = [[AVAudioSession sharedInstance] isOtherAudioPlaying];
    }
    return audioIsPlaying;
}

void _SetAudioExclusive(bool audioExclusive)
{
    NSError* err = nil;
    
    if (audioExclusive)
    {
        //  This one does ducking
        //[[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategoryPlayback withOptions:AVAudioSessionCategoryOptionDuckOthers error:nil];
        //[[AVAudioSession sharedInstance] setMode:AVAudioSessionModeMoviePlayback error:nil];

        //  This one exclusive
        [[AVAudioSession sharedInstance] setCategory:AVAudioSessionCategorySoloAmbient withOptions:AVAudioSessionCategoryOptionDuckOthers error:nil];
        [[AVAudioSession sharedInstance] setMode:AVAudioSessionModeDefault error:nil];
        
        [[AVAudioSession sharedInstance] setActive:YES withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation error:&err];
    }
    else
    {
        //  Don't do anything - can throw exception - but when successful it would resume other apps
        //[[AVAudioSession sharedInstance] setActive:NO withOptions:AVAudioSessionSetActiveOptionNotifyOthersOnDeactivation error:&err];
    }
}

void _StopAudioFromOtherApps()
{
    //  Stops music player from other apps
    _SetAudioExclusive(true);
}

// ------------------------------------------------------------------------------