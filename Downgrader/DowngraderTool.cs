using GBX.NET;
using GBX.NET.Engines.Game;
using GbxToolAPI;
using System.Runtime.CompilerServices;

namespace Downgrader;

[ToolName("Downgrader")]
[ToolDescription("Downgrade a TM2 Stadium Map to TMNF")]
[ToolAuthors("LinuxCat")]
[ToolGitHub("LinUwUxCat/Downgrader")]
[ToolAssets("Downgrader")]
[ToolAssetsIgnoreIngame("BlockList")]
public class DowngraderTool : ITool, IHasOutput<NodeFile<CGameCtnChallenge>>, IHasAssets{

    private CGameCtnChallenge map;
    private String[] CanGoToZero = new String[]{"StadiumPool","StadiumWater","StadiumDirtBorder","StadiumDirt"}; //Ideally this shouldn't be hardcoded
    public BlockList? TMNF { get; private set; } // public will be useful on the web
    public EditsList? Edits { get; private set; }
    public DowngraderTool(CGameCtnChallenge map){
        this.map = map;
    }

    public static string RemapAssetRoute(string route, bool isManiaPlanet)
    {
        return ""; // everything should stay the same
    }

    public async ValueTask LoadAssetsAsync()
    {
        TMNF = await AssetsManager<DowngraderTool>.GetFromYmlAsync<BlockList>(Path.Combine("BlockList", "TMNF.yml"));
        Edits = await AssetsManager<DowngraderTool>.GetFromYmlAsync<EditsList>(Path.Combine("Edits", "Edits.yml"));
    }

    public NodeFile<CGameCtnChallenge> Produce(){

        /* checks */
        if (!GameVersion.IsManiaPlanet(map)) throw new("Only Maniaplanet maps are supported!");
        if (map.Collection != "Stadium") throw new("Only TM2 Stadium maps are supported!");
        if (map.PlayerModel != null && !new String[]{"American", "SnowCar", "Rally", "SportCar", "CoastCar", "BayCar", "StadiumCar", ""}.Contains(map.PlayerModel.Id)) throw new("The provided car isn't supported!");
        if (TMNF is null) throw new("TMNF.yml has not been retrieved in time!");

        /* Removing: */

        //Author stuff
        map.HeaderChunks.Remove(0x03043008); //Remove "author" header chunk
        map.Chunks.Remove(0x03043042); //Remove "author" body chunk
        map.AuthorExtraInfo = null;
        map.AuthorNickname = null;
        map.AuthorVersion = null;
        map.AuthorZone = null;
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
        if (map.Decoration!=null){
            string mood = map.Decoration.Id;
            if (mood.Contains("Night"))   mood = "Night";
            if (mood.Contains("Sunset"))  mood = "Sunset";
            if (mood.Contains("Day"))     mood = "Day";
            if (mood.Contains("Sunrise")) mood = "Sunrise";
            map.Decoration = map.Decoration with { Id = mood };
        }
        
        //EmbeddedData
        map.Chunks.Remove(0x03043054); 
        //Genealogies
        map.Chunks.Remove(0x03043043); 
        //LightMap stuff
        map.Chunks.Remove(0x0304303D);
        //mapstyle
        map.MapStyle = null;
        map.MapType = null;
        //CP
        map.NbCheckpoints = null;
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
        if (map.ChallengeParameters != null){
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
        map.Collection = "Stadium";
        
        //Header chunks versions
        var x2 = map.HeaderChunks.Get<CGameCtnChallenge.Chunk03043002>();
        if(x2 != null){
            x2.Version = 11;
            map.HeaderChunks.Add(x2);
        }

        var x3 = map.HeaderChunks.Get<CGameCtnChallenge.Chunk03043003>();
        if(x3 != null){
            x3.Version = 5;
            map.HeaderChunks.Add(x3);
        }

        //PackDesc
        if (map.CustomMusicPackDesc != null){
            map.CustomMusicPackDesc = map.CustomMusicPackDesc with {Version = 2};
        } 

        if (map.ModPackDesc != null){
            map.ModPackDesc = map.ModPackDesc with {Version = 2};
        }

        //Swap out block chunk
        map.Chunks.Remove(0x0304301F);
        map.Chunks.Create(0x03043013);
        //Map size
        map.Size = new Int3(32,32,32);

        //Blocks
        if (map.Blocks == null) map.Blocks = new List<CGameCtnBlock>();
        else map.Blocks = downgradeBlockList(map.Blocks);

        //Mediatracker
        map.ClipIntro = downgradeClip(map.ClipIntro);
        map.ClipGroupInGame = downgradeClipGroup(map.ClipGroupInGame);
        map.ClipGroupEndRace = downgradeClipGroup(map.ClipGroupEndRace);

        map.MapName = "[D]" + map.MapName;

        return new(map, map.MapName + ".Challenge.Gbx", false);
    }

    CGameCtnMediaClip? downgradeClip(CGameCtnMediaClip? clip){
        if (clip == null) return null;

        clip.Chunks.Remove<CGameCtnMediaClip.Chunk0307900D>();
        clip.Chunks.Create(0x03079004);
        clip.Chunks.Create(0x03079005);
        clip.Chunks.Create(0x03079007);

        foreach (var track in clip.Tracks){
            track.Chunks.Remove<CGameCtnMediaTrack.Chunk03078005>();
            track.Chunks.Create<CGameCtnMediaTrack.Chunk03078004>();
            
            var newBlocks = new List<CGameCtnMediaBlock>();
            foreach (var block in track.Blocks){

                //MediaBlockDOF -> MediaBlockFxBlurDepth
                if (block is CGameCtnMediaBlockDOF dof){
                    CGameCtnMediaBlockFxBlurDepth fxBlurDepth = (CGameCtnMediaBlockFxBlurDepth)RuntimeHelpers.GetUninitializedObject(typeof(CGameCtnMediaBlockFxBlurDepth));
                    fxBlurDepth.Chunks.Create<CGameCtnMediaBlockFxBlurDepth.Chunk03081001>();
                    var newKeys = new List<CGameCtnMediaBlockFxBlurDepth.Key>();
                    foreach(var key in dof.Keys){
                        CGameCtnMediaBlockFxBlurDepth.Key newKey = new();
                        newKey.Time = key.Time;
                        newKey.ForceFocus = true;
                        newKey.FocusZ = key.ZFocus;
                        newKey.LensSize = key.LensSize;
                        newKeys.Add(newKey);
                    }
                    fxBlurDepth.Keys = newKeys.ToArray();
                    newBlocks.Add(fxBlurDepth);
                    continue;
                }

                //Ignore all new blocks
                //Note : BloomHdr is not FxBloom - dunno if it could be converted like DOF -> FxBlurDepth
                if (!MediaBlocks.TMNF.Contains(block.GetType()))continue;

                //Downgrade compatible blocks
                if (block is CGameCtnMediaBlockCameraPath cameraPath){
                    cameraPath.Chunks.Remove<CGameCtnMediaBlockCameraPath.Chunk030A1003>();
                    cameraPath.Chunks.Create<CGameCtnMediaBlockCameraPath.Chunk030A1002>();
                } else if (block is CGameCtnMediaBlockCameraCustom cameraCustom){
                    cameraCustom.Chunks.Remove<CGameCtnMediaBlockCameraCustom.Chunk030A2006>();
                    cameraCustom.Chunks.Create<CGameCtnMediaBlockCameraCustom.Chunk030A2005>();
                } else if (block is CGameCtnMediaBlockCameraGame cameraGame){
                    cameraGame.Chunks.Remove<CGameCtnMediaBlockCameraGame.Chunk03084007>();
                    cameraGame.Chunks.Create<CGameCtnMediaBlockCameraGame.Chunk03084003>();
                    if (cameraGame.gameCam1 != null){
                        switch (cameraGame.gameCam1){
                            case CGameCtnMediaBlockCameraGame.EGameCam.Internal:
                                cameraGame.gameCam = "Internal";
                                break;
                            case CGameCtnMediaBlockCameraGame.EGameCam.Behind:
                                cameraGame.gameCam = "Behind";
                                break;
                            default:
                                cameraGame.gameCam = "<Default>";
                                break;
                        }
                    }
                    if (cameraGame.gameCam2 != null){
                        switch (cameraGame.gameCam2){
                            case CGameCtnMediaBlockCameraGame.EGameCam2.Internal:
                                cameraGame.gameCam = "Internal";
                                break;
                            case CGameCtnMediaBlockCameraGame.EGameCam2.External:
                                cameraGame.gameCam = "Behind";
                                break;
                            default:
                                cameraGame.gameCam = "<Default>";
                                break;
                        }
                    }
                } else if (block is CGameCtnMediaBlockImage image){
                    image.Image = image.Image with { Version = 2 };
                } else if (block is CGameCtnMediaBlockSound sound){
                    var soundChunk003 = sound.GetChunk<CGameCtnMediaBlockSound.Chunk030A7003>();
                    if (soundChunk003 != null){
                        soundChunk003.Version = 0;
                        sound.Chunks.Add(soundChunk003);
                    }
                } else if (block is CGameCtnMediaBlockGhost mediaBlockGhost){
                    //Ignore for now.
                    continue;
                }
                newBlocks.Add(block);
            }
            track.Blocks = newBlocks;
        }
        return clip;
    }

    CGameCtnMediaClipGroup.Trigger downgradeTrigger(CGameCtnMediaClipGroup.Trigger trigger){
        for (int i = 0; i < trigger.Coords.Length; i++){
            trigger.Coords[i] = trigger.Coords[i] with { Y = trigger.Coords[i].Y - 8 };
        }
        return trigger;
        //TODO : Figure out how to deal with this shit
    }

    CGameCtnMediaClipGroup? downgradeClipGroup(CGameCtnMediaClipGroup? clipGroup){
        if (clipGroup == null)return null;
        for (int i = 0; i < clipGroup.Clips.Count; ++i){
            clipGroup.Clips[i] = new CGameCtnMediaClipGroup.ClipTrigger(downgradeClip(clipGroup.Clips[i].Clip), downgradeTrigger(clipGroup.Clips[i].Trigger));
        }
        return clipGroup;
    }

    CGameCtnBlock? downgradeBlock(CGameCtnBlock initialBlock){
        if (TMNF is null) throw new("TMNF.yml has not been retrieved in time!");
        if (Edits is null) throw new("Edits.yml has not been retrieved in time!");
        Int3 Offset = new(0, -8, 0); //TM2 maps are, by default, 8 blocks higher than TMNF's

        //If there is specific edits to be made, we do them
        if (Edits.Edits.ContainsKey(initialBlock.Name)){
            Offset = Edits.Edits[initialBlock.Name].IntOffset;
            string newName = Edits.Edits[initialBlock.Name].ReplaceWith;
            if (newName != "")initialBlock.Name = newName;
        } else { //If not, we check if the block is known
            if (!TMNF.Blocks.Contains(initialBlock.Name))return null; //Ignore all non-TMNF blocks
        }

        initialBlock.Bit17 = false;                     //
        initialBlock.Bit21 = false;                     // Some TM2-only behaviors
        initialBlock.WaypointSpecialProperty = null;    //
        
        initialBlock.Coord += Offset; //Apply offset
        if (initialBlock.Coord.X < 0 || initialBlock.Coord.X > 31 || initialBlock.Coord.Z < 0 || initialBlock.Coord.Z > 31)return null;
        if (initialBlock.Coord.Y <= 0 || initialBlock.Coord.Y >= 32){
            if (!(initialBlock.Coord.Y == 0 && CanGoToZero.Contains(initialBlock.Name)))return null; 
        }
        if (initialBlock.Skin != null){  //Changing packdesc version (yes, this causes a crash if not done)
            if (initialBlock.Skin.PackDesc != null)initialBlock.Skin.PackDesc = initialBlock.Skin.PackDesc with {Version = 2};
            if (initialBlock.Skin.ParentPackDesc != null)initialBlock.Skin.ParentPackDesc = initialBlock.Skin.ParentPackDesc with {Version = 2};
            initialBlock.Skin.Text = "";
        }
        return initialBlock;
    }

    IList<CGameCtnBlock> downgradeBlockList(IList<CGameCtnBlock> initialBlocks){
        var newBlockList = new List<CGameCtnBlock>();
        foreach (var block in initialBlocks){
            CGameCtnBlock? newBlock = downgradeBlock(block);
            if (newBlock != null)newBlockList.Add(newBlock);
        }        
        return newBlockList;
    }
}

