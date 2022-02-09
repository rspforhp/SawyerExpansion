using DiskCardGame;
using GBC;
using SawyerExpansion.Singleton;
using UnityEngine;

namespace SawyerExpansion.ClassesWithInstances
{
    public class BurnTheCard : AlternateInputInteractable
    {
        public override CursorType CursorType
        {
            get { return CursorType.Pickaxe; }
        }

        public override bool CollisionIs2D
        {
            get { return true; }
        }

        public override bool CanEnable()
        {
            return gameObject.GetComponent<PixelPlayableCard>().InHand;
        }


        public override void ManagedUpdate()
        {
            this.coll2D = this.gameObject.GetComponent<BoxCollider2D>();
        }

        public override void OnAlternateSelectStarted()
        {

            //if()
            {

                gameObject.GetComponent<PixelPlayableCard>()
                    .StartCoroutine(Singleton<SawyerResourceManager>.Instance.AddHeat(1));
                foreach (var card in Singleton<PixelPlayerHand>.Instance.CardsInHand)
                {
                    CustomCoroutine.Instance.StartCoroutine(card.TriggerHandler.OnTrigger(Trigger.Die, new object[]
                    {
                        true, gameObject.GetComponent<PixelPlayableCard>()
                    }));
                }

                foreach (var card in Singleton<PixelBoardManager>.Instance.CardsOnBoard)
                {
                    CustomCoroutine.Instance.StartCoroutine(card.TriggerHandler.OnTrigger(Trigger.Die, new object[]
                    {
                        true, gameObject.GetComponent<PixelPlayableCard>()
                    }));
                }

                CustomCoroutine.Instance.StartCoroutine(gameObject.GetComponent<PixelPlayableCard>().TriggerHandler
                    .OnTrigger(Trigger.Die, new object[]
                    {
                        true, null
                    }));

                Singleton<PixelPlayerHand>.Instance.RemoveCardFromHand(
                    gameObject.GetComponent<PixelPlayableCard>());
                gameObject.GetComponent<PixelPlayableCard>().Anim.PlayDeathAnimation();
                Destroy(gameObject.GetComponent<PixelPlayableCard>().gameObject);

            }

        }
    }

}