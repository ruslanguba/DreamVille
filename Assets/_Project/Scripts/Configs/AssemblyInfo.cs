#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;

// EditMode tests build catalog and rules assets in memory through the internal
// CreateForTests factories; the serialized fields themselves stay private.
[assembly: InternalsVisibleTo("DreamVille.Tests.EditMode")]
#endif
