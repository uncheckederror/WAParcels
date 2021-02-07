# 📦 AllParcels
This is an app that gathers the freshest copies of all the parcels in Washington State using primary data sources.

# 2020 Parcel Snapshot
The state publishes a yearly parcels snapshot here: 

https://geo.wa.gov/datasets/3dca0b72eae94c098b21329e1e61afd7_0

This project has the same goal, but rather than a one time snapshot this app will give you the freshest parcel data everytime it's run.

# Help Us Collect All the Parcels
Right now we don't have complete coverage of all the counties in Washington state.

If you know a GIS person that works for any of following counties please convince them to publish their parcel data on the web.

- [ ] Adams
- [ ] Asotin
- [ ] Benton
- [ ] Columbia
- [ ] Cowlitz
- [ ] Franklin
- [ ] Garfield
- [ ] Klickitat
- [ ] Lincoln
- [ ] Pend Oreille
- [ ] Skamania
- [ ] Stevens
- [ ] Wahkiakum
- [ ] Walla Walla
- [ ] Whatcom
- [ ] Whitman

If you know a GIS person that works for any of these counties please thank them for their commitment to open data.

- [x] Clallam
- [x] Jefferson
- [x] Grays Harbor
- [x] Pacific
- [x] Mason
- [x] Skagit
- [x] Island
- [x] Kitsap
- [x] Pierce
- [x] Thurston
- [x] Lewis
- [x] Clark
- [x] Snohomish
- [x] King
- [x] Okanogan
- [x] Chelan
- [x] Kittitas
- [x] Yakima
- [x] Grant
- [x] Ferry
- [x] Spokane

Here's the available parcel data in Washington State using only the web services provided by each county.

https://uncheckederror.github.io/KingCountyArcGISJS/wa-state-parcels.html

* 🟥 - No Data
* 🟨 - Inadequate Data
* Unshaded/Transparent - Useable Data (zoom in to view the parcels)

# How to use this app
1. Clone this repository to your machine.
2. Using a file browser navigate to the root directory of the project on your machine.
3. Open the .sln Solution file in Visual Studio 2019.
4. Click the green run button near the top-center of the Visual Studio window.
5. Watch the console prompt as the files are downloaded to your machine.
6. Using a file browser navigate to the file path provided as the final line in the console window (projectRoot/AllParcels).
7. Open the shapefiles in this folder in ArcMap, ArcGIS Pro, [QGIS](https://qgis.org/en/site/forusers/download.html#) or whatever tool you prefer.
8. Enjoy! 🚀

Note: You can also run this from command line or VSCode if you have the latest version of [dotnet](https://dotnet.microsoft.com/) installed by setting the current directory to this project and using the "dotnet run" command. There's no platform specific code in this project so it should run on MacOS and Linux without modification although this has not been tested.

If you find this project interesting or helpful please let me know.
