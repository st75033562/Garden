using Networking;
using Gameboard.Script;
using Robomation;

namespace Gameboard
{
    class SetEffectorHandler : RequestHandler<SetEffectorRequest>
    {
        public IRobotManager robotManager { get; set; }

        public override void Handle(ClientConnection conn, SetEffectorRequest request)
        {
            if (robotManager == null) { return; }

            var robot = robotManager.get(request.RobotIndex);
            if (robot != null)
            {
                var packet = new HamsterEffectorPacket(request.Data.ToByteArray());
                robot.write(Hamster.LEFT_WHEEL, packet.leftWheel);
                robot.write(Hamster.RIGHT_WHEEL, packet.rightWheel);
                robot.write(Hamster.BUZZER, packet.buzzerFrequency);
                robot.write(Hamster.NOTE, packet.note);
                robot.write(Hamster.LEFT_LED, packet.leftLed);
                robot.write(Hamster.RIGHT_LED, packet.rightLed);
            }
        }
    }
}
