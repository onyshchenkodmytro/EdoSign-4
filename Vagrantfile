Vagrant.configure("2") do |config|
  config.vm.define "edosign33" do |edosign|
    edosign.vm.box = "bento/ubuntu-22.04"
    edosign.vm.hostname = "edosign33-ubuntu"

    # Проброс портів
    edosign.vm.network "forwarded_port", guest: 7090, host: 7090, auto_correct: true
    edosign.vm.network "forwarded_port", guest: 7275, host: 7275, auto_correct: true

    edosign.vm.provision "shell", inline: <<-SHELL
      echo "=== Оновлення системи ==="
      apt-get update -y
      apt-get install -y wget git apt-transport-https ca-certificates lsb-release

      echo "=== Встановлення .NET SDK 9.0 ==="
      wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
      dpkg -i packages-microsoft-prod.deb
      apt-get update -y
      apt-get install -y dotnet-sdk-9.0

      echo "=== Клонування репозиторію EdoSign-4 ==="
      su - vagrant -c "rm -rf ~/EdoSign-4 && git clone https://github.com/onyshchenkodmytro/EdoSign-4 ~/EdoSign-4"

      echo "=== Копіювання коректного NuGet.Config ==="
      su - vagrant -c "mkdir -p ~/.nuget/NuGet"
      cp /vagrant/NuGet.Config /home/vagrant/.nuget/NuGet/NuGet.Config
      chown vagrant:vagrant /home/vagrant/.nuget/NuGet/NuGet.Config

      echo "=== Копіювання SQLite бази ==="
      # Якщо файл app.db існує в корені проекту — копіюємо
      if [ -f /vagrant/app.db ]; then
        echo " → Файл app.db знайдено. Копіюю до ~/main/app.db"
        su - vagrant -c "mkdir -p ~/main"
        cp /vagrant/app.db /home/vagrant/main/app.db
        chown vagrant:vagrant /home/vagrant/main/app.db
      else
        echo " ⚠ Файл app.db НЕ знайдено у /vagrant. SQLite створить порожню базу!"
      fi

      echo "=== Restore ==="
      su - vagrant -c "cd ~/EdoSign-4/EdoAuthServer && dotnet restore --no-dependencies"
      su - vagrant -c "cd ~/EdoSign-4/EdoSign.Lab-3 && dotnet restore --no-dependencies"

      echo "=== Publish EdoAuthServer ==="
      su - vagrant -c "cd ~/EdoSign-4/EdoAuthServer && dotnet publish -c Release -o ~/auth"

      echo '=== Publish EdoSign.Lab-3 ==='
      su - vagrant -c "cd ~/EdoSign-4/EdoSign.Lab-3 && dotnet publish -c Release -o ~/main"

      echo "=== Запуск EdoAuthServer (порт 7090) ==="
      su - vagrant -c "nohup dotnet ~/auth/EdoAuthServer.dll --urls=http://0.0.0.0:7090 > ~/auth.log 2>&1 &"

      echo "=== Запуск EdoSign.Lab-3 (порт 7275) ==="
      su - vagrant -c "nohup dotnet /home/vagrant/main/EdoSign.Lab-3.dll --contentroot /home/vagrant/main --urls=http://0.0.0.0:7275 > ~/main.log 2>&1 &"

      echo "==========================================================================="
      echo "=== ГОТОВО! ==="
      echo "=== Сервер аутентифікації: http://localhost:7090 ==="
      echo "=== Основний застосунок:   http://localhost:7275 ==="
      echo "==========================================================================="
    SHELL
  end
end
