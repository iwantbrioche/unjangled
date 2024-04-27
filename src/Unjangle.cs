using BepInEx;
using Menu.Remix.MixedUI;
using MonoMod.Cil;
using MoreSlugcats;
using System;
using System.IO;

namespace UnJangle
{
    [BepInPlugin(MOD_ID, "No More Jangling", "1.2")]
    public class UnJangle : BaseUnityPlugin
    {
        private UnJangleRemix unjangleRemix;
        private const string ATLASES_DIR = "unjangledAtlases";
        public const string MOD_ID = "iwantbread.unjangledmask";

        private bool IsInit;
        private bool PostIsInit;
        private void OnEnable()
        {
            unjangleRemix = new UnJangleRemix(this);
            On.RainWorld.OnModsInit += RainWorldOnModsInit;
            On.RainWorld.PostModsInit += RainWorldPostModsInit;
        }


        public void RainWorldOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (IsInit) return;

                bool atlasesLoaded = LoadAtlases();
                if (!atlasesLoaded)
                    Logger.LogWarning("Unjangled King Mask sprites not found! Reinstall the mod!");

                MachineConnector.SetRegisteredOI(MOD_ID, unjangleRemix);
                On.MoreSlugcats.VultureMaskGraphics.ctor_PhysicalObject_AbstractVultureMask_int += VultureMaskGraphics_ctor_1;
                On.MoreSlugcats.VultureMaskGraphics.ctor_PhysicalObject_MaskType_int_string += VultureMaskGraphics_ctor_2;
                On.MoreSlugcats.VultureMaskGraphics.DrawSprites += VultureMaskGraphics_DrawSprites;

                IsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }

        public void RainWorldPostModsInit(On.RainWorld.orig_PostModsInit orig, RainWorld self)
        {
            orig(self);
            try
            {
                if (PostIsInit) return;

                PostIsInit = true;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
                throw;
            }
        }



        public void VultureMaskGraphics_ctor_1(On.MoreSlugcats.VultureMaskGraphics.orig_ctor_PhysicalObject_AbstractVultureMask_int orig, VultureMaskGraphics self, PhysicalObject attached, VultureMask.AbstractVultureMask abstractMask, int firstSprite)
        {
            orig(self, attached, abstractMask, firstSprite);
            if (self.pearlStrings.Count > 0)
            {
                self.pearlStrings = new();
            }
        }
        public void VultureMaskGraphics_ctor_2(On.MoreSlugcats.VultureMaskGraphics.orig_ctor_PhysicalObject_MaskType_int_string orig, VultureMaskGraphics self, PhysicalObject attached, VultureMask.MaskType type, int firstSprite, string overrideSprite)
        {
            orig(self, attached, type, firstSprite, overrideSprite);
            if (self.pearlStrings.Count > 0)
            {
                self.pearlStrings = new();
            }
        }
        private void VultureMaskGraphics_DrawSprites(On.MoreSlugcats.VultureMaskGraphics.orig_DrawSprites orig, VultureMaskGraphics self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, UnityEngine.Vector2 camPos)
        {
            orig(self, sLeaser, rCam, timeStacker, camPos);

            if (!unjangleRemix.ToggleAntlers.Value) return;

            for (int i = 0; i < (self.King ? 4 : 3); i++)
            {
                FAtlasElement maskElement = GetCustomElementFromName(sLeaser.sprites[self.firstSprite + i], "KingMask", "Trimmed");
                if (maskElement != null)
                {
                    sLeaser.sprites[self.firstSprite + i].element = maskElement;
                }
            }
        }

        public static FAtlasElement GetCustomElementFromName(FSprite toCopy, string toReplace, string elementPrefix)
        {
            string elementName = toCopy?.element?.name;
            if (elementName == null) return null;

            if (elementPrefix == "") return null;

            if (!elementName.StartsWith(toReplace)) return null;

            FAtlasElement element = Futile.atlasManager.GetElementWithName(elementPrefix + elementName);

            return element;
        }

        public static bool LoadAtlases()
        {
            string[] atlasPaths = AssetManager.ListDirectory(ATLASES_DIR);
            foreach (string filePath in atlasPaths)
            {
                if (Path.GetExtension(filePath) == ".txt")
                {
                    string atlasName = Path.GetFileNameWithoutExtension(filePath);
                    FAtlas atlas = Futile.atlasManager.LoadAtlas(ATLASES_DIR + Path.AltDirectorySeparatorChar + atlasName);
                    if (atlas == null) return false;
                }
            }
            return true;
        }

    }

    public class UnJangleRemix : OptionInterface
    {
        public UnJangleRemix(UnJangle unJangle)
        {
            ToggleAntlers = config.Bind("UnJangle_ToggleAntlers", true, new ConfigurableInfo("Disable Chieftain Mask Antlers"));
        }

        public readonly Configurable<bool> ToggleAntlers;

        public override void Initialize()
        {
            base.Initialize();

            var opTab = new OpTab(this, "Options");
            Tabs = [opTab];

            opTab.AddItems(
                new OpCheckBox(ToggleAntlers, new(10f, 500f)),
                new OpLabel(45f, 504f, "Disable Chieftain Mask Antlers")
                );
        }

        public override void Update()
        {
            base.Update();
        }
    }
}