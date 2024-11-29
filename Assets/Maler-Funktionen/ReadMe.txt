Einfügen der Maler-Tools in ein Minerva-Projekt:

1. Assets/Maler-Funktionen/Prefabs öffnen.

2. "Maler Tools" und "PaintInfoUI" in die Scene einfügen.

3. "InitializationUI" in "MixedRealitySceneContent" einfügen.

4. "PainterMenuButton" in "MixedRealitySceneContent" und dann in "ExtraMenus_Small_HideOnHandDrop" oder "HandMenus_Small_HideOnHandDrop" unter MenuContent/MainButtons einfügen.

5. "MainButtons" oder "ButtonCollection" auswählen, und in der GridObjectCollection-Komponente "Update Collection" klicken.

6. Das eingefügte "PainterMenuButton" auswählen und in der "Interactable"-Komponente unter Events(OnClick) "PaintManager" verlinken (Child von "MalerTools") und als Aktion "PaintManager.toggleColorMenu" auswählen.

7. In "InitializationUI" für "StopScan" und "StopOrigin" jeweils in der "Interactable"-Komponente unter Events(OnClick) "PaintInitialization" verlinken (Child von "MalerTools") 
und als Aktion für "StopScan" "RoomMeshScanManager.OnFinishMeshScan" und für "StopOrigin" "OriginPointManager.FinishSetOrigin" auswählen.

8. Im "PaintManager" (unter "MalerTools") für "Color Mode Info" das eingefügte "PaintInfoUI"-Objekt verlinken.

9. Für "Nullpunkt" das "Main Camera"-Objekt verlinken.

10. Gegebenfalls unter "Managers" oder "NewManagers" das "Initialization"-Objekt deaktivieren (falls vorhanden) oder Überflüssige Skripte deaktivieren.

11. In "PaintInitialization" im Origin Point Manager für Origin "SpatialOriginReference" und für Reference Object "Main Camera" verlinken.

12. Unter "Managers" in "Utils" für den Scene Object Manager für "MeshScanUI" "MeshScan" und für "Origin UI" "Origin" aus "InitializationUI" verlinken.