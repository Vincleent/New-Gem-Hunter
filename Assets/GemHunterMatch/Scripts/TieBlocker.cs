using UnityEngine;

namespace Match3
{
    public class VirusBlocker : Obstacle
    {
        [Header("Virus Settings")]
        [Range(0f, 1f)] public float SpreadChance = 0.5f; // 每次步數後的蔓延機率

        public override void Init(Vector3Int cell)
        {
            m_CurrentState = 0;
            base.Init(cell);

            // 初始化：註冊 Cell 並上鎖
            Board.RegisterCell(cell);
            Board.ChangeLock(cell, true);
            Board.RegisterMatchedCallback(cell, CellMatch);
        }

        private void Start()
        {
            if (LevelData.Instance != null)
                LevelData.Instance.OnMoveHappened += OnMove;
        }


        public override void Clear()
        {
            Board.UnregisterMatchedCallback(m_Cell, CellMatch);
            Board.ChangeLock(m_Cell, false);

            // 移除監聽，避免遊戲繼續呼叫已被刪掉的物件
            LevelData.Instance.OnMoveHappened -= OnMove;

            Destroy(gameObject);
        }

        void CellMatch()
        {
            if (ChangeState(m_CurrentState + 1))
            {
                Clear();
            }
        }

        void OnMove(int remainingMoves)
        {
            TrySpread();
        }

        void TrySpread()
        {
            double rand = Random.value;
            if (rand > SpreadChance) return;

            Vector3Int dir = rand switch
            {
                >= 0.88 => Vector3Int.up,
                >= 0.76 => Vector3Int.down,
                >= 0.64 => Vector3Int.left,
                _ => Vector3Int.right
            };

            // 上下左右方向
            var targetCell = m_Cell + dir;

            if (Board.Instance.CellContent.TryGetValue(targetCell, out var content))
            {
                // 如果該格沒有障礙物
                if (content.Obstacle == null)
                {
                    // 建立新的 VirusBlocker
                    var newVirus = Instantiate(this, Board.Instance.GetCellCenter(targetCell), Quaternion.identity);
                    newVirus.Init(targetCell);
                }
            }
        }
    }
}
