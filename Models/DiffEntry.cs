namespace BinaryDifference.Models;

/// <summary>Raw diff result from the binary comparison engine.</summary>
public record DiffEntry(long Offset, string Hex1, string Hex2);