# 📦 WAParcels
This is an app that gathers the freshest copies of all the parcels in Washington State using primary data sources.

[Get the parcels here!](https://waparcels.tax/)

![image](https://user-images.githubusercontent.com/11726956/107158443-db1e0a80-693e-11eb-847d-a32401cc0c66.png)

# 2020 Parcel Snapshot
The state publishes a [yearly parcels snapshot](https://geo.wa.gov/datasets/3dca0b72eae94c098b21329e1e61afd7_0).

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

# How to build and run this project
1. Clone this repository to your machine.
2. Using a file browser navigate to the root directory of the project on your machine.
3. Open the .sln Solution file in Visual Studio 2019.
4. Click the green run button near the top-center of the Visual Studio window.
![image](https://user-images.githubusercontent.com/11726956/107158380-6a76ee00-693e-11eb-92c4-cb605f3ecc80.png)
5. Watch the console prompt as the files are downloaded to your machine.
![image](https://user-images.githubusercontent.com/11726956/107158354-4b785c00-693e-11eb-806b-63e78e166e74.png)
6. Using a file browser navigate to the file path provided as the final line in the console window (projectRoot/WA).
7. Open the shapefiles in this folder in ArcMap, ArcGIS Pro, [QGIS](https://qgis.org/en/site/forusers/download.html#) or whatever tool you prefer.
![image](https://user-images.githubusercontent.com/11726956/107158443-db1e0a80-693e-11eb-847d-a32401cc0c66.png)
8. Enjoy! 🚀

Note: You can also run this from command line or VSCode if you have the latest version of [dotnet](https://dotnet.microsoft.com/) installed by setting the current directory to this project and using the "dotnet run" command. There's no platform specific code in this project so it should run on MacOS and Linux without modification although this has not been tested.

If you find this project interesting or helpful please let me know.

# Roadmap 2023
- [ ] Use GDAL's ogr2ogr to merge all of these shapefiles into a single file.
- [ ] Use tippecanoe to create a vector tileset from this parcel data.
- [ ] Use Protomaps to create a static tileset and display it in a map on the github pages site for this repo.
