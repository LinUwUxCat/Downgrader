using GBX.NET;
using GBX.NET.Engines.Game;
using GbxToolAPI;
using Blocklist;

namespace Downgrader;

[ToolName("Downgrader")]
[ToolDescription("Downgrade a TM2 Stadium Map to TMNF")]
[ToolAuthors("LinuxCat")]
[ToolGitHub("LinUwUxCat/Downgrader")]
public class DowngraderTool : ITool, IHasOutput<NodeFile<CGameCtnChallenge>>{

    private CGameCtnChallenge map;

    public DowngraderTool(CGameCtnChallenge map){
        this.map = map;
    }


    public NodeFile<CGameCtnChallenge> Produce(){

        /* checks */
        if (!GameVersion.IsManiaPlanet(map))throw new("Only Maniaplanet maps are supported!");
        if (map.Collection != "Stadium")throw new("Only TM2 Stadium maps are supported!");
        if (map.PlayerModel!=null&& !new String[]{"American", "SnowCar", "Rally", "SportCar", "CoastCar", "BayCar", "StadiumCar", ""}.Contains(map.PlayerModel.Id))throw new("The provided car isn't supported!");

        /* Removing: */
        
        //Author stuff
        map.HeaderChunks.Remove(0x03043008); //Remove "author" header chunk
        map.Chunks.Remove(0x03043042); //Remove "author" body chunk
        map.AuthorExtraInfo=null;
        map.AuthorNickname=null;
        map.AuthorVersion=null;
        map.AuthorZone=null;
        //BakedBlocks stuff
        map.Chunks.Remove(0x03043048); 
        //Bot Path stuff
        map.Chunks.Remove(0x03043053); 
        //Title info stuff
        map.Chunks.Remove(0x03043051); 
        //CarMarksBuffer
        map.Chunks.Remove(0x0304303E); 
        //Clip stuff
        map.Chunks.Remove(0x03043049); 
        //Time stuff
        map.Chunks.Remove(0x03043056); 
        //Deco stuff
        map.Chunks.Remove(0x03043052);
        //EmbeddedData
        map.Chunks.Remove(0x03043054); 
        //Genealogies
        map.Chunks.Remove(0x03043043); 
        //LightMap stuff
        map.Chunks.Remove(0x0304303D);
        //mapstyle
        map.MapStyle=null;
        map.MapType=null;
        //CP
        map.NbCheckpoints=null;
        //ObjectiveText
        map.Chunks.Remove(0x0304304B); 
        //Offzone
        map.Chunks.Remove(0x03043050); 
        //Metadata
        map.Chunks.Remove(0x03043044);
        //AnchoredObjects
        map.Chunks.Remove(0x03043040);
        //Random useless chunks
        map.Chunks.Remove(0x03043034);
        map.Chunks.Remove(0x03043036);
        map.Chunks.Remove(0x03043038);
        map.Chunks.Remove(0x0304304F);
        map.Chunks.Remove(0x03043055);
        map.Chunks.Remove(0x03043057);
        map.Chunks.Remove(0x03043058);
        map.Chunks.Remove(0x03043059);
        //ChallengeParameters
        if (map.ChallengeParameters!=null){
            map.ChallengeParameters.Chunks.Remove<CGameCtnChallengeParameters.Chunk0305B00A>();
            map.ChallengeParameters.Chunks.Remove<CGameCtnChallengeParameters.Chunk0305B00D>();
            map.ChallengeParameters.Chunks.Remove<CGameCtnChallengeParameters.Chunk0305B00E>();
        }

        /* adding : */

        //Checkpoints
        map.Chunks.Create(0x03043017); //add "checkpoints"
        //Mediatracker
        map.Chunks.Create(0x03043021); //Add "legacy mediatracker"
        //Play mode
        map.Chunks.Create(0x0304301C); //Add "play mode"
        //Collection
        map.Collection="Stadium";
        
        //Header chunks versions
        var x2 = map.HeaderChunks.Get<CGameCtnChallenge.Chunk03043002>();
        if(x2!=null){
            x2.Version = 11;
            map.HeaderChunks.Add(x2);
        }
        var x3 = map.HeaderChunks.Get<CGameCtnChallenge.Chunk03043003>();
        if(x3!=null){
            x3.Version = 5;
            map.HeaderChunks.Add(x3);
        }
        //PackDesc
        if (map.CustomMusicPackDesc!=null){
            map.CustomMusicPackDesc = map.CustomMusicPackDesc with {Version=2};
        } 
        if (map.ModPackDesc!=null){
            map.ModPackDesc = map.ModPackDesc with {Version=2};
        }

        
        //Blocks
        //Swap out chunk
        map.Chunks.Remove(0x0304301F);
        map.Chunks.Create(0x03043013);
        //Map size
        map.Size = new Int3(32,32,32);

        if (map.Blocks==null){ //Why would anyone do this
            map.Blocks = new List<CGameCtnBlock>();
        } else {
            var newBlockList = new List<CGameCtnBlock>();
            foreach (var block in map.Blocks){
                var yOffset = 8; //TM2 maps are 8 blocks higher than TMNF's

                //Block-specific changes
                if (block.Name=="StadiumWater2"){block.Name="StadiumWater";yOffset--;}                  //TM2 added a StadiumWater2 and StadiumPool2.
                if (block.Name=="StadiumPool2"){block.Name="StadiumPool";yOffset--;}                    //The only thing they do is they have no offset, so i rename the block and cancel the offset.
                if (block.Name == "StadiumCircuitToRoadMain")block.Name = "StadiumPlatformToRoadMain";  //TM2 made that block Circuit (Cube platform) instead of Platform, so we can just rename it.

                if (!TMNF.Blocks.Contains(block.Name))continue; //Ignore all non-TMNF blocks

                block.Bit17=false;                      //
                block.Bit21=false;                      // Some TM2-only behaviors
                block.WaypointSpecialProperty = null;   //
                
                switch (block.Name){ //Stupid block changes, because these don't have the same offset in TM2
                    case "StadiumPool":
                    case "StadiumWater":
                    case "StadiumDirtBorder":
                    case "StadiumDirt":
                        yOffset++;
                        break;
                    case "StadiumDirtHill":
                        yOffset--;
                        break;
                    default:
                        break;
                }

                block.Coord += (0,-yOffset,0); //Apply offset

                if (block.Coord.Y <= 0 || block.Coord.Y >= 32){
                    if (!(block.Coord.Y == 0 && new String[]{"StadiumPool","StadiumWater","StadiumDirtBorder","StadiumDirt"}.Contains(block.Name)))continue; 
                }

                if (block.Skin!=null){  //Changing packdesc version (yes, this causes a crash if not done)
                    if (block.Skin.PackDesc!=null)block.Skin.PackDesc=block.Skin.PackDesc with {Version=2};
                    if (block.Skin.ParentPackDesc!=null)block.Skin.ParentPackDesc=block.Skin.ParentPackDesc with {Version=2};
                    block.Skin.Text = "";
                }

                newBlockList.Add(block);
            }
            map.Blocks = newBlockList;
        }

        map.ClipIntro=null;
        map.ClipGroupInGame=null;
        map.ClipGroupEndRace=null;

        map.MapName = "[D]" + map.MapName;

        return new(map, map.MapName + "Challenge.Gbx", false);
    }
}
