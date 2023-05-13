using System.Collections.Generic;
using System.Collections.ObjectModel;
using GBX.NET.Engines.Game;
namespace Blocklist{
    class TMNF{
        public static HashSet<Type> MediaBlocks = new HashSet<Type>{
            typeof(CGameCtnMediaBlockTriangles2D),
            typeof(CGameCtnMediaBlockCameraEffectShake),
            typeof(CGameCtnMediaBlockTransitionFade),
            typeof(CGameCtnMediaBlockTrails),
            typeof(CGameCtnMediaBlockCameraPath),
            typeof(CGameCtnMediaBlockCameraCustom),
            typeof(CGameCtnMediaBlockCameraGame), //Camera race
            typeof(CGameCtnMediaBlockFxBlurMotion),
            typeof(CGameCtnMediaBlockFxBlurDepth),
            typeof(CGameCtnMediaBlockFxBloom),
            typeof(CGameCtnMediaBlockFxColors),
            typeof(CGameCtnMediaBlock3dStereo),
            typeof(CGameCtnMediaBlockMusicEffect), //Music volume
            typeof(CGameCtnMediaBlockText),
            typeof(CGameCtnMediaBlockImage),
            typeof(CGameCtnMediaBlockSound),
            typeof(CGameCtnMediaBlockGhost)
        };
    }

    class TMNESWC{
         public static HashSet<Type> MediaBlocks = new HashSet<Type>{
            typeof(CGameCtnMediaBlockCameraEffectShake),
            typeof(CGameCtnMediaBlockTransitionFade),
            typeof(CGameCtnMediaBlockCameraPath), //Camera -> camera path
            typeof(CGameCtnMediaBlockCameraCustom), //Camera -> camera custom
            typeof(CGameCtnMediaBlockCameraGame), //Camera -> Camera race
            typeof(CGameCtnMediaBlockFxBlurDepth), //FX blur
            typeof(CGameCtnMediaBlockFxColors),
            typeof(CGameCtnMediaBlockMusicEffect), //Music volume
            typeof(CGameCtnMediaBlockText),
            typeof(CGameCtnMediaBlockImage),
            typeof(CGameCtnMediaBlockSound)
        };
    }
}