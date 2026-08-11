using System;
using System.IO;
using System.Linq;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.Character;
using ithappy.Creative_Characters_FREE.CharacterCustomizationTool.Editor.MaterialManagement;
using UnityEditor;
using UnityEngine;

namespace LogiCard.EditorTools
{
    /// <summary>
    /// Replaces the Quaternius-sourced <see cref="PawnImportTool"/> pipeline for Scout/Juggernaut with
    /// an assembly from the ithappy "Creative Characters FREE" pack (docs/ART_PACK_RESEARCH.md
    /// Characters section) — a modular slot-based system (Body/Faces/Outerwear/Pants/Shoes/Hairstyle/
    /// Hat/Gloves/...), not a single finished FBX per archetype, so <see cref="PawnImportTool"/>'s
    /// one-FBX-in, shader-convert, bake-prefab shape doesn't fit; this drives the pack's own
    /// <see cref="CustomizableCharacter"/> assembly API directly (the same API its
    /// <c>CharacterCustomizationWindow</c> Editor GUI drives) instead of hand-stitching FBX parts, per
    /// this project's own discipline of reusing a pack's intended workflow
    /// (<c>InteriorPackImportTool</c>/<c>PawnImportTool</c> precedent).
    ///
    /// No shader conversion step is needed here (unlike <see cref="PawnImportTool"/>) — every part in
    /// this pack shares one already-URP/Lit material (<c>Materials/Color.mat</c>, shader guid
    /// 933532a4fcc9baf4fa0491de14d08ed7, confirmed by direct inspection — same shader already verified on
    /// this project's other archetype materials), and <see cref="CustomizableCharacter"/>'s own assembly
    /// forces every part's <c>SkinnedMeshRenderer.sharedMaterial</c> onto that one material regardless of
    /// which parts are picked, matching the pack's texture-atlas-driven design (not per-part unique
    /// materials).
    /// </summary>
    public static class CharacterCustomizationImportTool
    {
        [MenuItem("Tools/LogiCard/Import Scout (Creative Characters Pack)")]
        public static void ImportScoutBatch()
        {
            ImportArchetype("Resources/Scout", "Scout", BuildScout);
        }

        [MenuItem("Tools/LogiCard/Import Juggernaut (Creative Characters Pack)")]
        public static void ImportJuggernautBatch()
        {
            ImportArchetype("Resources/Juggernaut", "Juggernaut", BuildJuggernaut);
        }

        /// <summary>
        /// Scout — lean/civilian/recon-coded (ART_DIRECTION "Scout vs Juggernaut readable"): the pack's
        /// only Outfit-group option (<c>Outfit_010</c>, a fitted single-piece civilian outfit — there is
        /// no second Outfit variant to choose between), the smaller-bbox <c>Pants_009</c>, the
        /// smallest-bbox <c>Shoe_Slippers_005</c>, and visible <c>Hairstyle_Male_005</c> hair with no
        /// headgear at all — bare head reads lean/uncovered next to Juggernaut's helmet below. No
        /// Gloves/Glasses/Accessories (bare hands, clean silhouette; the pack's Face Accessories group is
        /// clown-nose/headphones/pacifier novelty items, unsuited to either archetype).
        /// </summary>
        private static void BuildScout(CustomizableCharacter character)
        {
            character.ToDefault();
            character.PickGroup(GroupType.Faces, 2, true); // Male_emotion_usual_001 — neutral/calm
            character.PickGroup(GroupType.Outfit, 0, true); // Outfit_010 — only Outfit-group option
            character.PickGroup(GroupType.Pants, 0, true); // Pants_009 — smaller bbox than Pants_010
            character.PickGroup(GroupType.Shoes, 1, true); // Shoe_Slippers_005 — smallest-bbox shoe
            character.PickGroup(GroupType.Hairstyle, 1, true); // Hairstyle_Male_005 — visible hair, no hat
        }

        /// <summary>
        /// Juggernaut — bulky/armored-coded. This agent has no screenshot/Editor-interactive access this
        /// session (docs/PAWN_ART_REWORK_PLAN.md's Verification note), so instead of guessing bulk from
        /// filenames, every ambiguous pick below was chosen by comparing
        /// <c>SkinnedMeshRenderer.sharedMesh.bounds</c> volume across that slot's variants
        /// (Tools/LogiCard/Diagnostics/Log Character Part Bounds) and taking the largest:
        /// <c>Outwear_050</c> (largest of 3 Outwear jackets, beats Outwear_004/043), <c>Pants_010</c>
        /// (larger of 2), <c>Shoe_Sneakers_009</c> (largest of 3 shoe options), <c>Hat_Single_016</c>
        /// (largest of all 4 Hat/HatSingle options — reads as the bulkiest headgear), <c>Gloves_014</c>
        /// (larger of 2, ~50% bigger bbox than Gloves_006). Hairstyle is left off; the Hat covers the head
        /// instead of Scout's visible hair, another silhouette-contrast lever.
        /// </summary>
        private static void BuildJuggernaut(CustomizableCharacter character)
        {
            character.ToDefault();
            character.PickGroup(GroupType.Faces, 0, true); // Male_emotion_angry_003 — intense
            character.PickGroup(GroupType.Outwear, 2, true); // Outwear_050 — largest-bbox jacket
            character.PickGroup(GroupType.Pants, 1, true); // Pants_010 — larger-bbox pants
            character.PickGroup(GroupType.Shoes, 2, true); // Shoe_Sneakers_009 — largest-bbox shoe
            character.PickGroup(GroupType.HatSingle, 2, true); // Hat_Single_016 — largest-bbox headgear
            character.PickGroup(GroupType.Gloves, 1, true); // Gloves_014 — larger-bbox gloves
        }

        private static void ImportArchetype(string archetypeFolder, string archetypeName, Action<CustomizableCharacter> configure)
        {
            SlotLibrary slotLibrary = SlotLibraryLoader.LoadSlotLibrary();
            if (slotLibrary == null)
            {
                Debug.LogError("CharacterCustomizationImportTool: could not load SlotLibrary asset.");
                return;
            }

            var character = new CustomizableCharacter(slotLibrary);
            configure(character);

            GameObject instance = character.InstantiateCharacter();
            var materialProvider = new MaterialProvider();

            // Same mesh/material assignment CharacterCustomizationWindow.SavePrefab does — swap each
            // enabled slot's chosen mesh onto the base rig's matching named child (Body/Outerwear/Hat/...)
            // and force every part onto the pack's one shared URP/Lit material. FirstOrDefault (not the
            // window's First) so a slot with no matching child logs instead of throwing.
            int meshesAssigned = 0;
            foreach (SlotBase slot in character.Slots.Where(s => s.IsEnabled))
            {
                foreach ((SlotType partType, Mesh mesh) in slot.Meshes)
                {
                    Transform child = instance.transform.Cast<Transform>()
                        .FirstOrDefault(t => t.name.StartsWith(partType.ToString()));
                    if (child == null || !child.TryGetComponent(out SkinnedMeshRenderer skinnedMeshRenderer))
                    {
                        Debug.LogWarning($"CharacterCustomizationImportTool: no '{partType}' child renderer on the base mesh — {mesh.name} not assigned.");
                        continue;
                    }

                    skinnedMeshRenderer.sharedMesh = mesh;
                    skinnedMeshRenderer.sharedMaterial = materialProvider.MainColor;
                    skinnedMeshRenderer.localBounds = skinnedMeshRenderer.sharedMesh.bounds;
                    meshesAssigned++;
                }
            }

            // Base_Mesh.prefab ships with a live Animator + Character_Movement.controller already wired
            // (its own default state, not something this tool or the window's AddAnimator step adds) —
            // strip it back to inert. This brief is a mesh/rig swap only (docs/PAWN_ART_REWORK_PLAN.md:
            // "don't wire animation playback"); leaving a controller assigned risks idle-state root motion
            // silently drifting the pawn's visual within its parent transform, which no batchmode test
            // would catch (no rendering, no multi-second play window) and this agent cannot screenshot to
            // verify. The Animator component itself is left in place — inert, but keeps the door open for
            // a future real animation-wiring pass.
            if (instance.TryGetComponent(out Animator animator))
            {
                animator.runtimeAnimatorController = null;
                animator.applyRootMotion = false;
            }

            string folder = $"Assets/_Project/Art/Characters/{archetypeFolder}";
            Directory.CreateDirectory(folder);

            string prefabPath = $"{folder}/{archetypeName}.prefab";
            PrefabUtility.SaveAsPrefabAsset(instance, prefabPath);
            UnityEngine.Object.DestroyImmediate(instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"CharacterCustomizationImportTool: built {prefabPath} ({meshesAssigned} part mesh(es) assigned, material=Color.mat, Animator controller cleared).");
        }
    }
}
