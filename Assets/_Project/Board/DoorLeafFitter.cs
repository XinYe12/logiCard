using UnityEngine;

namespace LogiCard.Board
{
    /// <summary>
    /// Fits a door-leaf mesh hierarchy under a hinge so Open/Closed yaw reads as a hinged swing.
    /// Nappin <c>(Prb)Door</c> authors width on local Z and thickness on X; BoardView swings around Y
    /// with width on X — without this re-orient, the leaf spins like a cabinet slab.
    /// </summary>
    public static class DoorLeafFitter
    {
        /// <summary>
        /// Parents <paramref name="leaf"/> under a new Mount on <paramref name="hinge"/>, rotates if
        /// the leaf's dominant horizontal extent is Z, then scales/positions so width runs +X from
        /// the hinge edge (x=0), floor sits at y=0, and thickness is centered on z=0.
        /// </summary>
        /// <returns>The Mount transform (direct child of the hinge).</returns>
        public static Transform FitUnderHinge(
            Transform hinge,
            Transform leaf,
            float targetWidth,
            float targetHeight,
            float targetThickness)
        {
            var mountGo = new GameObject("Mount");
            Transform mount = mountGo.transform;
            mount.SetParent(hinge, false);
            mount.localPosition = Vector3.zero;
            mount.localRotation = Quaternion.identity;
            mount.localScale = Vector3.one;

            leaf.SetParent(mount, false);
            leaf.localPosition = Vector3.zero;
            leaf.localRotation = Quaternion.identity;
            leaf.localScale = Vector3.one;

            Bounds bounds = CalculateBoundsInAncestor(mount, leaf.gameObject);
            if (bounds.size.sqrMagnitude < 1e-8f)
            {
                return mount;
            }

            // Width-on-Z (nappin Door) → yaw so width lands on X. SeparatorDoor is already X-wide.
            if (bounds.size.z > bounds.size.x + 1e-4f)
            {
                leaf.localRotation = Quaternion.Euler(0f, 90f, 0f);
                bounds = CalculateBoundsInAncestor(mount, leaf.gameObject);
            }

            float sx = bounds.size.x > 1e-4f ? targetWidth / bounds.size.x : 1f;
            float sy = bounds.size.y > 1e-4f ? targetHeight / bounds.size.y : 1f;
            float sz = bounds.size.z > 1e-4f ? targetThickness / bounds.size.z : 1f;
            mount.localScale = new Vector3(sx, sy, sz);

            // Position in hinge space: non-uniform scale is on Mount (identity rotation), so
            // hingePoint = mountPos + scale * mountLocalPoint.
            mount.localPosition = new Vector3(
                -sx * bounds.min.x,
                -sy * bounds.min.y,
                -sz * bounds.center.z);

            return mount;
        }

        /// <summary>
        /// Axis-aligned bounds of all meshes under <paramref name="subject"/>, expressed in
        /// <paramref name="ancestor"/> local space (ancestor scale/rotation must be identity for
        /// the fit math above; children may carry the axis-fix yaw).
        /// </summary>
        public static Bounds CalculateBoundsInAncestor(Transform ancestor, GameObject subject)
        {
            var filters = subject.GetComponentsInChildren<MeshFilter>();
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            bool has = false;
            Matrix4x4 ancestorWorldToLocal = ancestor.worldToLocalMatrix;

            for (int i = 0; i < filters.Length; i++)
            {
                MeshFilter filter = filters[i];
                Mesh mesh = filter.sharedMesh;
                if (mesh == null)
                {
                    continue;
                }

                Matrix4x4 localToAncestor = ancestorWorldToLocal * filter.transform.localToWorldMatrix;
                Vector3 min = mesh.bounds.min;
                Vector3 max = mesh.bounds.max;
                Vector3[] corners =
                {
                    min,
                    max,
                    new Vector3(min.x, min.y, max.z),
                    new Vector3(min.x, max.y, min.z),
                    new Vector3(max.x, min.y, min.z),
                    new Vector3(min.x, max.y, max.z),
                    new Vector3(max.x, min.y, max.z),
                    new Vector3(max.x, max.y, min.z),
                };

                for (int c = 0; c < corners.Length; c++)
                {
                    Vector3 p = localToAncestor.MultiplyPoint3x4(corners[c]);
                    if (!has)
                    {
                        bounds = new Bounds(p, Vector3.zero);
                        has = true;
                    }
                    else
                    {
                        bounds.Encapsulate(p);
                    }
                }
            }

            return bounds;
        }
    }
}
