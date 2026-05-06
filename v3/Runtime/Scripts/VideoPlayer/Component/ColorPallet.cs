using UdonSharp;

namespace Kinel.VideoPlayer.V3.Scripts.VideoPlayer.Component
{
    /// <summary>
    /// 使い方：予定
    /// https://gihyo.jp/design/serial/01/ui-design-unsung/0019
    /// メインカラー、サブカラー、アクセントカラー、背景色を設定するスクリプト
    /// Childにこのスクリプトがある場合は上書きするようにする。
    ///   - これはチェックボックスで制御
    ///   - 自身より親にこのスクリプトがある場合は何も行わない
    /// Editor拡張で即時反映させたいりビルド時に設定したり。
    /// 将来的にcoreに移して他のギミックと共通化してもいいかもしれない
    /// </summary>
    public class ColorPallet : UdonSharpBehaviour
    {
        
    }
}