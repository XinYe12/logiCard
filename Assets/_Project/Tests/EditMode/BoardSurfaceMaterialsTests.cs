using LogiCard.Board;
using NUnit.Framework;
using UnityEngine;

namespace LogiCard.Tests.EditMode
{
    /// <summary>Map Phase 2 / C65 — board surfaces use the flat/toon family, not photographic wet-PBR.</summary>
    public sealed class BoardSurfaceMaterialsTests
    {
        [TestCase(MapSurfaceRole.Yard)]
        [TestCase(MapSurfaceRole.Hall)]
        [TestCase(MapSurfaceRole.Vault)]
        [TestCase(MapSurfaceRole.Flank)]
        public void SurfaceMaterialFor_ReturnsFlatFamily_PerRole(MapSurfaceRole role)
        {
            Material material = BoardView.SurfaceMaterialFor(role);
            Assert.That(material, Is.Not.Null);
            Assert.That(
                BoardSurfaceMaterials.IsFlatFamily(material, allowGradientRamp: false),
                Is.True,
                $"{role} floor must be Solid()-family (no photographic _BaseMap).");
        }

        [Test]
        public void BrickWall_And_WoodEdge_AreFlatFamily()
        {
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.BrickWall, false), Is.True);
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.WoodEdge, false), Is.True);
        }

        [Test]
        public void FenceRail_And_FencePost_AreFlatFamily()
        {
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.FenceRail, false), Is.True);
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.FencePost, false), Is.True);
        }

        [Test]
        public void DoorLeaf_And_PropBody_AreFlatOrGradientFamily()
        {
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.DoorLeaf), Is.True);
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.PropBody), Is.True);
            Assert.That(BoardSurfaceMaterials.IsFlatFamily(BoardSurfaceMaterials.PropAccent), Is.True);
        }

        [Test]
        public void ForRole_Matches_SurfaceMaterialFor()
        {
            Assert.That(BoardSurfaceMaterials.ForRole(MapSurfaceRole.Yard),
                Is.SameAs(BoardView.SurfaceMaterialFor(MapSurfaceRole.Yard)));
            Assert.That(BoardSurfaceMaterials.ForRole(MapSurfaceRole.Hall),
                Is.SameAs(BoardView.SurfaceMaterialFor(MapSurfaceRole.Hall)));
            Assert.That(BoardSurfaceMaterials.ForRole(MapSurfaceRole.Vault),
                Is.SameAs(BoardView.SurfaceMaterialFor(MapSurfaceRole.Vault)));
            Assert.That(BoardSurfaceMaterials.ForRole(MapSurfaceRole.Flank),
                Is.SameAs(BoardView.SurfaceMaterialFor(MapSurfaceRole.Flank)));
        }
    }
}
