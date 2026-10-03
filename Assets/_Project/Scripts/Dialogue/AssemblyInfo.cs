#if UNITY_INCLUDE_TESTS
using System.Runtime.CompilerServices;

// EditMode tests build dialogue definitions in memory through the internal
// DialogueDefinition.Create factory; the serialized fields themselves stay private.
[assembly: InternalsVisibleTo("DreamVille.Tests.EditMode")]
#endif
