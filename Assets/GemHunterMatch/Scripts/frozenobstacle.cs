using UnityEngine;

namespace Match3
{
    public class IceBlocker : Obstacle
    {
        [Header("Spread Settings")]
        [Range(0f, 1f)] public float SpreadChance = 0.5f;

        protected bool m_WasMatchedThisTurn = false;

        protected bool m_Frozen = false;

        public override void Init(Vector3Int cell)
        {
            m_CurrentState = 3;

            Board.RegisterCell(cell);
            Board.ChangeLock(cell, true);
            foreach (var neighbour in BoardCell.Neighbours)
            {
                var adjacent = cell + neighbour;
                Board.RegisterDeletedCallback(adjacent, CellMatch);
            }


            base.Init(cell);

        }

        private void Start()
        {
            if (LevelData.Instance != null)
                LevelData.Instance.OnMoveHappened += OnMove;

            Board.OnAfterMatchTicking += ChangeSprite;
            var content = Board.Instance.CellContent[m_Cell];
            content.CanMatched = false;
        }


        public override void Clear()
        {
            foreach (var neighbour in BoardCell.Neighbours)
            {
                var adjacent = m_Cell + neighbour;
                Board.UnregisterDeletedCallback(adjacent, CellMatch);
            }

            Board.ChangeLock(m_Cell, false);

            // 移除監聽，避免遊戲繼續呼叫已被刪掉的物件
            LevelData.Instance.OnMoveHappened -= OnMove;
            Board.OnAfterMatchTicking -= ChangeSprite;

            Destroy(gameObject);
        }

        void Frozen()
        {
            foreach (var neighbour in BoardCell.Neighbours)
            {
                var adjacent = m_Cell + neighbour;
                Board.UnregisterDeletedCallback(adjacent, CellMatch);
            }

            LevelData.Instance.OnMoveHappened -= OnMove;
            Board.OnAfterMatchTicking -= ChangeSprite;
        }

        void CellMatch()
        {
            if (m_WasMatchedThisTurn)
            {
                if (ChangeState(m_CurrentState + 1))
                {
                    Clear();
                }
            }
            else
            {
                if (ChangeState(m_CurrentState + 2))
                {
                    Clear();
                }
            }

            m_WasMatchedThisTurn = true;
        }

        void OnMove(int remainingMoves)
        {
            //發生cell match就不動，反之加厚
            if (ChangeState(m_CurrentState - 1))
            {
                Frozen();
            }

            if (m_CurrentState < LockState.Length)
            {
                // TrySpread();
            }
            m_WasMatchedThisTurn = false;
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
                    var newIce = Instantiate(this, Board.Instance.GetCellCenter(targetCell), Quaternion.identity);
                    newIce.Init(targetCell);
                }
            }
        }

        protected override bool ChangeState(int newState)
        {
            //if done we return false as we don't want to re-delete it
            if (m_Done || m_Frozen)
                return false;

            m_CurrentState = newState;

            if (m_CurrentState < LockState.Length && m_CurrentState >= 0)
            {
                return false;
            }

            m_Done = true;
            return true;
        }

        void ChangeSprite()
        {
            if (m_CurrentState - 1 >= 0)
                GameManager.Instance.PoolSystem.PlayInstanceAt(LockState[m_CurrentState - 1].UndoneVFX, transform.position);

            if (m_CurrentState < LockState.Length  && m_CurrentState >= 0)
            {
                m_SpriteRenderer.sprite = LockState[m_CurrentState].Sprite;
            }

        }
    }
}
