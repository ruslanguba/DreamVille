using UnityEngine;

namespace DreamVille.Progression
{
    /// <summary>
    /// Состав уровня: единственное место, где сцена перечисляет своих участников.
    /// Перекрёстных ссылок между объектами сцены больше нет — они собраны здесь,
    /// где видны целиком. Автопоиск не годится: в лобби финальных зон две,
    /// и настоящая из них только одна.
    /// </summary>
    public class LevelActors : MonoBehaviour
    {
        [SerializeField] private FinalZone _finalZone;

        /// <summary>Зона, открывающая проход на следующий уровень. Может отсутствовать.</summary>
        public FinalZone FinalZone => _finalZone;
    }
}
