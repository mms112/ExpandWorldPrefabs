using System;
using ExpandWorld.Prefab;
using NUnit.Framework;

namespace ExpandWorldPrefabs.Tests;

public class BiomeFilterTests
{
  [TestCase("")]
  [TestCase(" ")]
  [TestCase("Meadows,BlackForest")]
  [TestCase("8")]
  [TestCase("None")]
  public void BaseParsing_PreservesExistingMasks(string value)
  {
    Assert.That(Helper.ToBiomeFilter(value, true, [], out var alts), Is.EqualTo(Helper.ToBiomes(value, true)));
    Assert.That(alts, Is.Null);
  }

  [Test]
  public void UnknownName_StillFailsValidation()
  {
    Assert.Throws<InvalidOperationException>(() => Helper.ToBiomeFilter("Kalhygge typo", true, [], out _));
  }
}