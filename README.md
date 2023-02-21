# UnityUtilsHelpers

Place this script into Assets/Editor folder
* Restart Editor
* Open project
* If you have Assets/Plugins/Android/res or Assets/Plugins/Android/assets folder present, you'll see a dialog asking about upgrade
* Basically Unity will move Assets/Plugins/Android/res -> Assets/Plugins/Android/res-legacy,Assets/Plugins/Android/assets -> Assets/Plugins/Android/assets-legacy and upon export/build will manually copy Assets/Plugins/Android/res-legacy to unityLibrary/src/main/res folder and Assets/Plugins/Android/assets-legacy to unityLibrary/src/main/assets folder (excluding .meta files)
