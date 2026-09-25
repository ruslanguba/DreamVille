using NUnit.Framework;

using DreamVille.Gameplay.Weapons;
using DreamVille.Tests.EditMode.Fakes;

namespace DreamVille.Tests.EditMode.Weapons
{
    public class WeaponControllerTests
    {
        [Test]
        public void FirePressed_AfterInitialize_CallsTryShoot()
        {
            var input = new FakePlayerInput();
            var weapon = new FakeWeapon();
            var controller = new WeaponController(input, weapon);
            controller.Initialize();

            input.RaiseFirePressed();

            Assert.That(weapon.TryShootCount, Is.EqualTo(1));
        }

        [Test]
        public void FirePressed_AfterDispose_IsIgnored()
        {
            var input = new FakePlayerInput();
            var weapon = new FakeWeapon();
            var controller = new WeaponController(input, weapon);
            controller.Initialize();
            controller.Dispose();

            input.RaiseFirePressed();

            Assert.That(weapon.TryShootCount, Is.Zero);
        }
    }
}
