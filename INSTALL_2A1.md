# Installing the WILDTYPE 2A.1 source patch

1. Exit Play Mode, save the project and close Unity.
2. Make a local Git commit or copy of `C:\Users\jsayl\Desktop\UnityProjects\WILDTYPE`.
3. Extract this ZIP to a temporary folder.
4. Copy the extracted `Assets` folder and three root Markdown files into:

   `C:\Users\jsayl\Desktop\UnityProjects\WILDTYPE`

5. Allow Windows to merge folders and replace the listed existing files. Do not delete the project's existing `Assets` folder.
6. Open the project with Unity 6000.4.7f1 and wait for compilation.
7. If the Console contains a compiler error, stop and report the complete first error before running the scene.
8. Open **Window > General > Test Runner** and run all Edit Mode tests. Expected discovery is 44 tests, but only Unity can confirm the result.
9. Run the existing Play Mode test once.
10. Open `Assets/WildType/Scenes/CreatureStage_Prototype.unity` for a short smoke test.

The patch deliberately does not create live mating or offspring. Normal gameplay should remain unchanged while the new foundation tests become available.
