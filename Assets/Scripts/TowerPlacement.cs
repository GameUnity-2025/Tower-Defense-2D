using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    private Tower _placedTower;
    private HashSet<Vector2> _occupiedPositions = new HashSet<Vector2>(); // Danh sách vị trí đã chiếm

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    // Fungsi yang terpanggil sekali khi ada object Rigidbody yang menyentuh area collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_placedTower != null || _occupiedPositions.Contains(transform.position))
        {
            return; // Ngăn đặt tháp nếu đã có tháp hoặc vị trí bị chiếm
        }

        Tower tower = collision.GetComponent<Tower>();
        if (tower != null)
        {
            tower.SetPlacePosition(transform.position);
            _placedTower = tower;
            _occupiedPositions.Add(transform.position); // Đánh dấu vị trí đã chiếm
        }
    }

    // Kebalikan dari OnTriggerEnter2D, fungsi ini terpanggil sekali khi object tersebut meninggalkan area collider
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (_placedTower == null)
        {
            return;
        }

        if (collision.GetComponent<Tower>() == _placedTower)
        {
            _placedTower.SetPlacePosition(null);
            _occupiedPositions.Remove(transform.position); // Giải phóng vị trí khi tháp rời đi
            _placedTower = null;
        }
    }

    // Phương thức để khóa tháp (gọi từ UI hoặc logic khác khi người chơi xác nhận đặt)
    public void LockTowerPlacement()
    {
        if (_placedTower != null)
        {
            _placedTower.LockPlacement();
            _placedTower = null; // Giải phóng tham chiếu sau khi đặt
        }
    }
}