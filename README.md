# AutoVerse – Interactive 3D Virtual Automobile Showroom

This lightweight Unity 6.6 starter generates five working scenes without downloading car models or environment packs. It uses simple built-in 3D shapes as clearly replaceable placeholders, plus a polished dark showroom UI.

## First launch

1. Open Unity Hub and choose **Add > Add project from disk**.
2. Select this `AutoVerse` folder. Let Unity finish importing its UI package.
3. In the Unity menu bar, choose **AutoVerse > Build All Scenes**.
4. Open `Assets/AutoVerse/Scenes/MainMenu.unity` if it is not already open, then press **Play**.

The scene builder creates Main Menu, Main Showroom, SUV Zone, EV Zone, and Customization Studio and registers all five for builds. Navigation, car rotation controls, door animation, headlights, paint colors, click-to-inspect, and the EV charging display are included.

## Notes

- This is a desktop 3D project. It does not require VR hardware.
- The model shapes are placeholders. Replace them later with small free FBX/GLB vehicle models if time and network permit; preserve the root transform and interaction objects.
- UI uses Unity's built-in uGUI package. No platform modules or large asset downloads are required.
- The project is intentionally stored on the HDD to preserve SSD space.
