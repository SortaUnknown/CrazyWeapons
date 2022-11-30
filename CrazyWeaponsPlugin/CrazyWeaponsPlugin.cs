using System;
using UnityEngine;
using BepInEx;

namespace CrazyWeaponsPlugin
{
    [BepInPlugin("HelloThere.MoreSpears", "Crazy Weapons", "1.1")]
    public class MoreSpearsMod : BaseUnityPlugin
    {
        public void OnEnable()
        {
            On.RainWorldGame.RawUpdate += RawUpdatePatch;
            On.Spear.HitSomething += SpearHitPatch;
            On.Rock.HitSomething += RockHitPatch;
            On.FlareBomb.HitWall += HitWallPatch;
            On.Spear.ApplyPalette += SpearApplyPalettePatch;
            On.Rock.ApplyPalette += RockApplyPalettePatch;
            On.FlareBomb.ApplyPalette += FlareApplyPalettePatch;
        }

        public static AbstractPhysicalObject storedRock;
        public static Creature storedCreature;

        private static void RawUpdatePatch(On.RainWorldGame.orig_RawUpdate orig, RainWorldGame self, float dt)
        {
            orig.Invoke(self, dt);

            if (Input.GetKeyDown("1"))
            {
                EntityID newID = self.GetNewID();
                newID.number = 100;
                AbstractSpear abstractSpear = new AbstractSpear(self.world, null, self.Players[0].pos, newID, false);
                abstractSpear.RealizeInRoom();
            }
            else if (Input.GetKeyDown("2"))
            {
                EntityID newID = self.GetNewID();
                newID.number = 101;
                AbstractSpear abstractSpear = new AbstractSpear(self.world, null, self.Players[0].pos, newID, false);
                abstractSpear.RealizeInRoom();
            }
            else if (Input.GetKeyDown("3"))
            {
                EntityID newID = self.GetNewID();
                newID.number = 102;
                AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.Players[0].pos, newID);
                abstractPhysicalObject.RealizeInRoom();
            }
            else if (Input.GetKeyDown("4"))
            {
                EntityID newID = self.GetNewID();
                newID.number = 103;
                if (storedRock != null)
                {
                    storedRock.realizedObject.Destroy();
                }
                storedRock = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.Rock, null, self.Players[0].pos, newID);
                storedRock.RealizeInRoom();
            }
            else if (Input.GetKeyDown("5") && storedRock != null && storedCreature != null)
            {
                AbstractCreature abstractCreature = new AbstractCreature(self.world, storedCreature.Template, null, storedRock.pos, storedCreature.abstractCreature.ID);
                abstractCreature.RealizeInRoom();
            }
            else if (Input.GetKeyDown("6"))
            {
                EntityID newID = self.GetNewID();
                newID.number = 104;
                AbstractPhysicalObject abstractPhysicalObject = new AbstractPhysicalObject(self.world, AbstractPhysicalObject.AbstractObjectType.FlareBomb, null, self.Players[0].pos, newID);
                abstractPhysicalObject.RealizeInRoom();
            }
            else if (Input.GetKeyDown("7"))
            {
                AbstractSpear abstractSpear = new AbstractSpear(self.world, null, self.Players[0].pos, self.GetNewID(), true);
                abstractSpear.RealizeInRoom();
            }
        }

        private static bool SpearHitPatch(On.Spear.orig_HitSomething orig, Spear self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool res = orig.Invoke(self, result, eu);
            if (result.obj is Creature)
            {
                if (IsKill(self)) { (result.obj as Creature).Die(); }
                else if (IsHeal(self))
                {
                    if ((result.obj as Creature).State is HealthState) { ((result.obj as Creature).State as HealthState).health = 1f; }
                    if ((result.obj as Creature).dead)
                    {
                        (result.obj as Creature).dead = false;
                        (result.obj as Creature).State.alive = true;
                        self.room.game.session.creatureCommunities.InfluenceLikeOfPlayer((result.obj as Creature).Template.communityID, self.room.world.region.regionNumber, (self.room.game.Players[0].realizedCreature as Player).playerState.playerNumber, 0.1f, 0.5f, 0f);
                    }
                }
            }
            return res;
        }

        private static bool RockHitPatch(On.Rock.orig_HitSomething orig, Rock self, SharedPhysics.CollisionResult result, bool eu)
        {
            bool res = orig.Invoke(self, result, eu);
            if (result.obj != null && IsErase(self)) { result.obj.Destroy(); }
            else if (IsCopy(self) && result.obj is Creature) { storedCreature = result.obj as Creature; }
            return res;
        }

        private static void HitWallPatch(On.FlareBomb.orig_HitWall orig, FlareBomb self)
        {
            orig.Invoke(self);
            if (IsGrenade(self))
            {
                foreach (AbstractCreature abstractCreature in self.room.abstractRoom.creatures)
                {
                    if (!(abstractCreature.realizedCreature is Player)) { abstractCreature.realizedCreature.Die(); }
                }
            }
        }

        private static void SpearApplyPalettePatch(On.Spear.orig_ApplyPalette orig, Spear self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (IsKill(self))
            {
                self.color = Color.red;
                sLeaser.sprites[0].color = Color.red;
            }
            else if (IsHeal(self))
            {
                self.color = Color.green;
                sLeaser.sprites[0].color = Color.green;
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        private static void RockApplyPalettePatch(On.Rock.orig_ApplyPalette orig, Rock self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (IsErase(self))
            {
                self.color = Color.magenta;
                sLeaser.sprites[0].color = Color.magenta;
                sLeaser.sprites[1].color = Color.magenta;
            }
            else if (IsCopy(self))
            {
                self.color = Color.blue;
                sLeaser.sprites[0].color = Color.blue;
                sLeaser.sprites[1].color = Color.blue;
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        private static void FlareApplyPalettePatch(On.FlareBomb.orig_ApplyPalette orig, FlareBomb self, RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, RoomPalette palette)
        {
            if (IsGrenade(self))
            {
                sLeaser.sprites[0].color = new Color(1f, 1f, 1f);
                sLeaser.sprites[2].color = Color.red;
            }
            else
            {
                orig.Invoke(self, sLeaser, rCam, palette);
            }
        }

        public static bool IsKill(Spear spear)
        {
            return spear.abstractPhysicalObject.ID.number == 100;
        }

        public static bool IsHeal(Spear spear)
        {
            return spear.abstractPhysicalObject.ID.number == 101;
        }

        public static bool IsErase(Rock rock)
        {
            return rock.abstractPhysicalObject.ID.number == 102;
        }

        public static bool IsCopy(Rock rock)
        {
            return rock.abstractPhysicalObject.ID.number == 103;
        }

        public static bool IsGrenade(FlareBomb bomb)
        {
            return bomb.abstractPhysicalObject.ID.number == 104;
        }
    }
}
