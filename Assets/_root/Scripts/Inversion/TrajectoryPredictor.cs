using Components;
using System.Collections.Generic;
using UnityEngine;

namespace Inversion
{
    public class TrajectoryPredictor
    {
        private readonly Collider _collider;
        private List<Collider> _worldColliders;
        private readonly float _radius;

        // Константа гравитации (можно брать Physics.gravity.magnitude)
        private readonly Vector3 _gravityAcceleration = new Vector3(0, -9.81f, 0);

        public TrajectoryPredictor(Collider collider)
        {
            _collider = collider;
            // Получаем эквивалентный радиус коллайдера (приближённо через сферу)
            _radius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z, collider.bounds.extents.y);
        }

        /// <summary>
        /// Создаёт слепок коллайдеров мира в заданном радиусе.
        /// </summary>
        public void CalculateWorldColliders(Vector3 center, float radius)
        {
            _worldColliders = new List<Collider>(Physics.OverlapSphere(center, radius));
            _worldColliders.Remove(_collider);
        }

        /// <summary>
        /// Предсказывает траекторию движения.
        /// </summary>
        /// <param name="start">Начальная позиция</param>
        /// <param name="initialImpulse">Начальная скорость (импульс)</param>
        /// <param name="attenuationTime">Время затухания импульса (0 = без затухания)</param>
        /// <param name="maxSteps">Максимальное число точек траектории</param>
        /// <param name="dt">Шаг симуляции (секунды)</param>
        public (List<Vector3> positions, List<Vector3> velocities) Predict(
            Vector3 start,
            Vector3 initialImpulse,
            float attenuationTime = 1f,
            int maxSteps = 500,
            float dt = 0.05f)
        {
            var positions = new List<Vector3> { start };
            var velocities = new List<Vector3> { initialImpulse };

            Vector3 pos = start;
            Vector3 vel = initialImpulse;
            float impulseProgress = 0f;
            bool isGrounded = false;

            for (int step = 0; step < maxSteps; step++)
            {
                // Обновляем затухание импульса (если нужно)
                if (attenuationTime > 0f && impulseProgress < 1f)
                {
                    impulseProgress += dt / attenuationTime;
                    float t = Mathf.Clamp01(impulseProgress);
                    vel = Vector3.Lerp(initialImpulse, Vector3.zero, t);
                }

                // Применяем гравитацию (только если объект не на земле)
                if (!isGrounded)
                {
                    vel += _gravityAcceleration * dt;
                }

                // Пробное перемещение
                Vector3 newPos = pos + vel * dt;

                // Проверка столкновения с миром (если есть коллайдеры)
                if (_worldColliders != null && CheckCollision(newPos, out Vector3 hitNormal))
                {
                    // Останавливаем объект при касании земли
                    isGrounded = true;
                    vel = Vector3.zero;
                    // Можно добавить коррекцию позиции, но для траектории достаточно остановки
                    break;
                }
                else
                {
                    isGrounded = false;
                    pos = newPos;
                }

                positions.Add(pos);
                velocities.Add(vel);

                // Условие остановки: скорость почти нулевая и объект на земле
                if (isGrounded && vel.magnitude < 0.01f)
                    break;
            }

            return (positions, velocities);
        }

        /// <summary>
        /// Проверяет, пересекает ли объект (сфера радиуса _radius) какой‑либо коллайдер из мира.
        /// Возвращает true и нормаль (условно) при столкновении.
        /// </summary>
        private bool CheckCollision(Vector3 position, out Vector3 normal)
        {
            normal = Vector3.up;
            foreach (var world in _worldColliders)
            {
                // Более точная проверка: пересекаются ли границы с учётом размера объекта
                Bounds objBounds = new Bounds(position, Vector3.one * _radius * 2f);
                if (objBounds.Intersects(world.bounds))
                {
                    // Упрощённо считаем столкновение
                    normal = (position - world.bounds.ClosestPoint(position)).normalized;
                    return true;
                }
            }
            return false;
        }
    }
}