<H1> 2D Tilemap Generator </H1>
<H2> Information about the contents of this repository </H2>
<paragraph>
This is an editor application, so it is designed to be used in the Unity editor, not compiled. Although, it can be easily integrated into an actual application.
To those unfamiliar with unity projects, the relevant code can found in the asset folder (2DTilemapGeneratorUnity -> Assets) as C# files.
  <H3> Quick installation guide </H3>
  <li> Get the contents of this repository </li>
  <li> Add the project to your unity hub </li>
  <li> Open project, unity will automatically generate required files </li>
  <li> In hierachy, you can find "TileMapGenerator" gameobject, which you can use to generate tilemaps during runtime </li>
</paragraph>

<paragraph>
  <H3> Usage of TilemapGenerator </H3>
  TilemapGenerator gameobject will contain following components: GenSettings, Physics Based Gen, CA Map Gen. <p></p>
  Here are the descriptions of changeable values in the editor:

  <H4> Gen Settings </H4>
  Defines the settings used in Physics Based Gen and CA Map Gen
  <li> Bounds define how large map you will generate and where </li>
  <li> Iterations defines how large a biome will be </li>
  <li> Probability defines how many biomes will there be </li>

  <H4> Physics Based Gen </H4>
  Generates singular river on a grass plane with flowers. Changes in the settings can cause it to generate lakes, as well.
  <li> Acc const defines how fast the river will change direction </li>
  <li> Offset defines how much more tiles are generated around the actual map</li>
  <li> Swap defines how often the river will swap target to change direction towards </li>
  <li> Swap head defines how often the river swaps point of the river head </li>
  <li> Tile amount gives how many tiles river will go through </li>
  <li> Flower probability defines how often a grass tile gets turned into a flower </li>
  <li> Gen is a boolean value, which when enabled will generate the map and be disabled </li>

  <H4> CA Map Gen </H4>
  Generates a 2D tilemap with shaded water, beaches, grass, forests and mountains.
  <li> Gen is a boolean value, which when enabled will generate the map and be disabled </li>
</paragraph>
